using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using ExitGames.Client.Photon;
using Game.Player;

namespace Game
{
	/// <summary>
	/// Keeps track of the current turn order and current acting <see cref="PunPlayer"/>.
	/// The MasterClient iterates this to advance the turn order.
	/// </summary>
	public class TurnIterator : MonoBehaviourPun, IEnumerator<PunPlayer>, IEnumerable<PunPlayer>
	{
		private const string TURN_CHANGE = nameof(OnTurnChangeRPC);

		public event UnityAction<TurnChangeEvent> OnTurnChange;

		public int CurrentTurn { get; private set; } = 0;
		public PunPlayer Current { get; private set; }
		object IEnumerator.Current => Current;

		private PunPlayer _currentStable;
		private PunPlayer _currentTemp;

		private void Awake() => GameManager.Instance.GameStart += OnGameStart;

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
				_currentStable = activePlayers.Single().Key;
				_currentTemp = default;
				return false;
			}
			else if (TryGetOutOfBoardPawn(out var pawn))
			{
				_currentTemp = pawn.Owner;
				InvokeOnTurnChange(_currentTemp);
				return true;
			}
			else if (_currentStable == activePlayers.Last().Key)
			{
				_currentStable = activePlayers.First().Key;
				_currentTemp = default;
				CurrentTurn++;
				InvokeOnTurnChange(_currentStable);
				return true;
			}
			else
			{
				_currentStable = activePlayers.ElementAt(GetIndexOf(_currentStable)+1).Key;
				_currentTemp = default;
				InvokeOnTurnChange(_currentStable);
				return true;
			}

			void InvokeOnTurnChange(PunPlayer current) => photonView.RPC(TURN_CHANGE, RpcTarget.All, new TurnChangeEvent(current, lastPlayer, CurrentTurn));

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

			bool TryGetOutOfBoardPawn(out Pawn pawn)
			{
				pawn = null;
				return false;
				throw new System.NotImplementedException(); // TODO Implement
			}
		}

		public void Reset() { }

		public void Dispose() { }

		public IEnumerator<PunPlayer> GetEnumerator() => this;

		IEnumerator IEnumerable.GetEnumerator() => this;

		[PunRPC]
		private void OnTurnChangeRPC(TurnChangeEvent turnChange, PhotonMessageInfo info)
		{
			if (!info.Sender.IsMasterClient || !turnChange.Valid)
				return;
			Current = turnChange.currentPlayer;
			CurrentTurn = turnChange.turn;
			OnTurnChange?.Invoke(turnChange);
		}

		/// <summary>
		/// Describes an event in the game in which the acting <see cref="PunPlayer"/> changes.
		/// </summary>
		public struct TurnChangeEvent
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

			public readonly bool Valid => currentPlayer != null && currentPlayer != lastPlayer && turn > 0;

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
		}
	}
}
