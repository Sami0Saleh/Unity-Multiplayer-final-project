using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace Game.Player
{
	public class PawnMovement : MonoBehaviourPun
	{
		public const int MAX_STEPS = 2;

		[SerializeField] private Pawn _pawn;

		public event UnityAction<PawnMovementEvent> OnPawnMoved;

		private void OnEnable()
		{
			TurnIterator.Instance.OnTurnChange += OnTurnChange;
		}

		private void OnDisable()
		{
			TurnIterator.Instance.OnTurnChange -= OnTurnChange;
		}

		private void OnTurnChange(TurnIterator.TurnChangeEvent turnChangeEvent)
		{
			if (!turnChangeEvent.Valid)
				return;
			if (turnChangeEvent.currentPlayer.IsLocal)
				ListenToCursorEvents(_pawn.Cursor);
			else
				UnListenToCursorEvents(_pawn.Cursor);

			void ListenToCursorEvents(Cursor cursor) => cursor.PositionPicked += OnPositionPicked;

			void UnListenToCursorEvents(Cursor cursor) => cursor.PositionPicked -= OnPositionPicked;
		}

		private void OnPositionPicked(byte position)
		{
			const string PAWN_MOVED = nameof(OnPawnMovedRPC);
			photonView.RPC(PAWN_MOVED, RpcTarget.All, new PawnMovementEvent(photonView.Owner, new byte[] { _pawn.Position, position })); // TODO Pass movement path
		}

		[PunRPC]
		private void OnPawnMovedRPC(PawnMovementEvent movement, PhotonMessageInfo info)
		{
			if (!movement.Valid || (info.Sender != movement.player && info.Sender != PhotonNetwork.MasterClient))
				return;
			OnPawnMoved?.Invoke(movement);
		}

		public struct PawnMovementEvent : IValidateable
		{
			[Tooltip("The player whose Pawn has moved.")] public PunPlayer player;
			[Tooltip("Contains the tiles on which this player's Pawn has stepped (Inclusive of both start and end tiles.)")] public byte[] steps;
			
			public readonly Pawn Pawn => GameManager.Instance.ActivePlayers[player];
			public readonly bool Valid => player != null && TurnIterator.Instance.Current == player && Pawn != null && steps.Length > 1 && steps.Distinct().Count() == steps.Length && Pawn.Position == steps.First() && Board.Instance.CurrentBoardState.Contains(steps.Last());

			public PawnMovementEvent(PunPlayer player, IEnumerable<byte> steps)
			{
				this.player = player;
				this.steps = steps.ToArray();
			}

			public PawnMovementEvent(PunPlayer player, byte[] steps)
			{
				this.player = player;
				this.steps = steps;
			}

			#region SERIALIZATION
			private const int MAX_ELEMENTS_IN_STEPS = MAX_STEPS + 1;
			private const int MAX_SIZE = sizeof(int) + (sizeof(byte) * MAX_ELEMENTS_IN_STEPS);
			private readonly short Size => (short)(sizeof(int) + steps.Length * sizeof(byte));
			private static readonly byte[] _bytes = new byte[MAX_SIZE];

			public static short Serialize(StreamBuffer outStream, object customobject)
			{
				var movement = (PawnMovementEvent)customobject;
				lock (_bytes)
				{
					int index = 0;
					Protocol.Serialize(movement.player.ActorNumber, _bytes, ref index);
					foreach (var step in movement.steps)
						_bytes[index++] = step;
					outStream.Write(_bytes, 0, movement.Size);
				}
				return movement.Size;
			}

			public static object Deserialize(StreamBuffer inStream, short length)
			{
				var movement = new PawnMovementEvent();
				lock (_bytes)
				{
					int index = 0;
					DeserializePlayer(ref index);
					DeserializeSteps(ref index);
				}
				return movement;

				void DeserializePlayer(ref int index)
				{
					inStream.Read(_bytes, index, length);
					Protocol.Deserialize(out int actorNumber, _bytes, ref index);
					movement.player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
				}

				void DeserializeSteps(ref int index)
				{
					int stepsCount = GetStepsCount();
					movement.steps = new byte[stepsCount];
					for (int step = 0; step < stepsCount; step++)
						movement.steps[step] = _bytes[index++];
				}

				int GetStepsCount() => length - sizeof(int);
			}
			#endregion
		}
	}
}
