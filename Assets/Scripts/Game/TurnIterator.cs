using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using ExitGames.Client.Photon;
using Game.Player;
using Game.Player.Visuals;

namespace Game
{
	/// <summary>
	/// Keeps track of the current turn order and current acting <see cref="PunPlayer"/>.
	/// The MasterClient iterates this to advance the turn order.
	/// </summary>
	public class TurnIterator : MonoBehaviourPun, IEnumerator<PunPlayer>, IEnumerable<PunPlayer>
	{
		public static TurnIterator Instance { get; private set; }

		private const string TURN_CHANGE = nameof(OnTurnChangeRPC);

		public event UnityAction<TurnChangeEvent> OnTurnChange;

		public int CurrentTurn { get; private set; } = 0;
		public PunPlayer Current => _currentTemp ?? _currentStable;
		object IEnumerator.Current => Current;

		private PunPlayer _currentStable;
		private PunPlayer _currentTemp;

		private void Awake()
		{
			if (!TryRegisterSingleton())
				return;
			GameManager.Instance.GameStart += OnGameStart;

			bool TryRegisterSingleton()
			{
				bool created = Instance == null;
				if (created)
					Instance = this;
				else
					Destroy(gameObject);
				return created;
			}
		}

		private void OnDestroy() => GameManager.Instance.GameStart -= OnGameStart;

		private void Start()
		{
			_currentStable = GameManager.Instance.ActivePlayers.Last().Key;
			MoveNext();
		}

		private void OnGameStart() => enabled = true;

		public bool MoveNext()
		{
			if (!PhotonNetwork.IsMasterClient)
				return true;
			var activePlayers = GameManager.Instance.ActivePlayers;
			PunPlayer lastPlayer = Current;
			if (activePlayers.Count == 1)
			{
				GameManager.Instance.TriggerGameOver(activePlayers.Single().Key);
				return false;
			}
			else
			{
				if (TryGetOutOfBoardPawn(out var pawn)) // Give priority to Pawns out of board
					InvokeOnTurnChange(new TurnChangeEvent(pawn.Owner, lastPlayer, CurrentTurn));
				else if (CurrentPlayerIsLast()) // Loop back to the first player and advance the turn counter
					InvokeOnTurnChange(new TurnChangeEvent(activePlayers.First().Key, lastPlayer, CurrentTurn + 1));
				else // Continue to next player normally
					InvokeOnTurnChange(new TurnChangeEvent(GetNextPlayer(), lastPlayer, CurrentTurn));
				return true;
			}

			int GetIndexOf(PunPlayer player)
			{
				int index = 0;
				foreach (var kvp in activePlayers)
				{
					if (kvp.Key == player)
						break;
					index++;
				}
				return index;
			}

			PunPlayer GetNextPlayer() => activePlayers.ElementAt(GetIndexOf(_currentStable) + 1).Key;

			bool CurrentPlayerIsLast() => _currentStable == activePlayers.Last().Key;

			bool TryGetOutOfBoardPawn(out Pawn pawn)
			{
				pawn = GameManager.Instance.ActivePlayers.Select(kvp => kvp.Value).FirstOrDefault(p => !p.IsOnBoard);
				return pawn != null;
			}
		}

		public void Reset() { }

		public void Dispose() { }

		public IEnumerator<PunPlayer> GetEnumerator() => this;

		IEnumerator IEnumerable.GetEnumerator() => this;

		private void InvokeOnTurnChange(TurnChangeEvent turnChangeEvent) => photonView.RPC(TURN_CHANGE, RpcTarget.All, turnChangeEvent);

		[PunRPC]
		private void OnTurnChangeRPC(TurnChangeEvent turnChange, PhotonMessageInfo info)
		{
			if (!info.Sender.IsMasterClient || !turnChange.Valid)
				return;
			var currentPlayer = turnChange.currentPlayer;
			var currentPawn = GameManager.Instance.ActivePlayers[currentPlayer];
			if (currentPawn.IsOnBoard)
			{
				_currentStable = currentPlayer;
				_currentTemp = default;
			}
			else
				_currentTemp = currentPlayer;
			CurrentTurn = turnChange.turn;
			OnTurnChange?.Invoke(turnChange);
			currentPawn.TurnEnd += InvokeNextTurn;

			void InvokeNextTurn(Pawn pawn)
			{
				pawn.TurnEnd -= InvokeNextTurn;
				MoveNext();
			}
		}

		/// <summary>
		/// Describes an event in the game in which the acting <see cref="PunPlayer"/> changes.
		/// </summary>
		public struct TurnChangeEvent : IValidateable
		{
			public PunPlayer currentPlayer;
			public PunPlayer lastPlayer;
			public int turn;

			public TurnChangeEvent(PunPlayer current, PunPlayer last, int currentTurn)
			{
				currentPlayer = current;
				lastPlayer = last;
				turn = currentTurn;
			}

			public readonly bool Valid => currentPlayer != null && turn > 0;

			#region SERIALIZATION
			private const int SIZE = 3 * sizeof(int);
			private static readonly byte[] _bytes = new byte[SIZE];

			public static short Serialize(StreamBuffer outStream, object customobject)
			{
				var turnChange = (TurnChangeEvent)customobject;
				lock (_bytes)
				{
					int index = 0;
					Protocol.Serialize(turnChange.currentPlayer.ActorNumber, _bytes, ref index);
					Protocol.Serialize(turnChange.lastPlayer.ActorNumber, _bytes, ref index);
					Protocol.Serialize(turnChange.turn, _bytes, ref index);
					outStream.Write(_bytes, 0, SIZE);
				}
				return SIZE;
			}

			public static object Deserialize(StreamBuffer inStream, short length)
			{
				var turnChange = new TurnChangeEvent();
				lock (_bytes)
				{
					int index = 0;
					inStream.Read(_bytes, index, length);
					Protocol.Deserialize(out int actorNumber, _bytes, ref index);
					turnChange.currentPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
					Protocol.Deserialize(out actorNumber, _bytes, ref index);
					turnChange.lastPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
					Protocol.Deserialize(out turnChange.turn, _bytes, ref index);
				}
				return turnChange;
			}
			#endregion

			public override readonly string ToString() => $"{currentPlayer} <= {lastPlayer} @ turn {turn}";
		}
	}
}
