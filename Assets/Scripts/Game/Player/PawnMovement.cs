using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using ExitGames.Client.Photon;
using PunPlayer = Photon.Realtime.Player;
using static Game.Board.BoardMask;

namespace Game.Player
{
	/// <summary>
	/// Handles sending and receiving <see cref="PawnMovementEvent"/>.
	/// </summary>
	public class PawnMovement : MonoBehaviourPun
	{
		public const int MAX_STEPS = 2;
		public const int STEPS_OUT_OF_BOARD = 1;

		[SerializeField] private Pawn _pawn;
		private readonly Stack<Position> _path = new(PawnMovementEvent.MAX_ELEMENTS_IN_STEPS);
		private int _stepsLeft;

		public event UnityAction<PawnMovementEvent> OnPawnMoved;
		public event UnityAction<IEnumerable<Position>> OnPathChanged;

		public bool HasMoved => _stepsLeft < MAX_STEPS;
		public bool AbleToMove => !ReachableArea.Empty() && _stepsLeft > 0;
		public Board.BoardMask ReachableArea => Pathfinding.GetTraversableArea(_pawn.Position, (byte)Math.Max(_stepsLeft, 0), Board.Instance.TraversableArea);

		private void Awake() => _pawn.TurnStart += OnStartTurn;

		private void OnDestroy() => _pawn.TurnStart -= OnStartTurn;

		private void OnEnable()
		{
			_path.Clear();
			_path.Push(_pawn.Position);
			_pawn.Cursor.PositionPicked += OnPositionPicked;
			_pawn.Cursor.PositionChanged += OnPositionChanged;
		}

		private void OnDisable()
		{
			_pawn.Cursor.PositionPicked -= OnPositionPicked;
			_pawn.Cursor.PositionChanged -= OnPositionChanged;
			_path.Clear();
		}

		private void OnStartTurn(Pawn pawn) => _stepsLeft = _pawn.IsOnBoard ? MAX_STEPS : STEPS_OUT_OF_BOARD;

		private void OnPositionPicked(Position position)
		{
			const string PAWN_MOVED = nameof(OnPawnMovedRPC);
			if (_path.Peek() == position)
				photonView.RPC(PAWN_MOVED, RpcTarget.All, new PawnMovementEvent(photonView.Owner, _path.Reverse()));
		}

		private void OnPositionChanged(Position position)
		{
			bool canBranch;
			if (_path.Peek() != position && _path.Contains(position))
				ReturnTo(position);
			else if (!ReachableArea.Contains(position) || !(canBranch = CanBranch(position, out var branchPosition)) || _path.Count >= _stepsLeft+1)
				return;
			else if (canBranch)
				BranchFrom(branchPosition, position);
			else
				_path.Push(position);
			OnPathChanged?.Invoke(_path.Reverse());

			void ReturnTo(Position position)
			{
				while (_path.Peek() != position && _path.Count > 1)
					_path.Pop();
			}

			bool CanBranch(Position to, out Position branchPosition)
			{
				branchPosition = _path.FirstOrDefault(p => CanBranchSingle(p, to));
				return branchPosition != default;
			}

			bool CanBranchSingle(Position from, Position to) => Pathfinding.GetNeighbors(from).Contains(to);

			void BranchFrom(Position branchPosition, Position to)
			{
				ReturnTo(branchPosition);
				_path.Push(to);
			}
		}

		[PunRPC]
		private void OnPawnMovedRPC(PawnMovementEvent movement, PhotonMessageInfo info)
		{
			if (!AbleToMove || !movement.Valid || (info.Sender != movement.player && info.Sender != PhotonNetwork.MasterClient))
				return;
			_stepsLeft -= (byte)(movement.steps.Count() - 1);
			OnPawnMoved?.Invoke(movement);
			if (_stepsLeft > 0)
			{
				_path.Clear();
				_path.Push(_pawn.Position);
			}
		}

		public struct PawnMovementEvent : IValidateable
		{
			[Tooltip("The player whose Pawn has moved.")] public PunPlayer player;
			[Tooltip("Contains the tiles on which this player's Pawn has stepped (Inclusive of both start and end tiles.)")] public Position[] steps;
			
			public readonly Pawn Pawn => GameManager.Instance.ActivePlayers[player];
			public readonly bool Valid
			{
				get
				{
					return PlayerValid(player) && StepsValid(steps, Pawn);

					static bool PlayerValid(PunPlayer player) => player != null && GameManager.Instance.ActivePlayers.ContainsKey(player) && TurnIterator.Instance.Current == player;

					static bool StepsValid(Position[] steps, Pawn pawn)
					{
						return HasEnoughSteps() && AllStepsAreValid();

						bool HasEnoughSteps() => steps.Length > 1 && steps.AllUnique();

						bool AllStepsAreValid() => pawn.Position == steps.First() && AllStepsAreReachable();

						bool AllStepsAreReachable() => pawn.Movement.ReachableArea.Contains(GetAllTraversedAreaExceptFirst());

						Board.BoardMask GetAllTraversedAreaExceptFirst() => FromPositions(steps) & ~pawn.Position.ToMask();
					}
				}
			}

			public readonly IEnumerable<Position> AllStepsButLast => steps.Take(steps.Length-1);

			public PawnMovementEvent(PunPlayer player, IEnumerable<Position> steps)
			{
				this.player = player;
				this.steps = steps.ToArray();
			}

			public PawnMovementEvent(PunPlayer player, Position[] steps)
			{
				this.player = player;
				this.steps = steps;
			}

			#region SERIALIZATION
			public const int MAX_ELEMENTS_IN_STEPS = MAX_STEPS + 1;
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
					movement.steps = new Position[stepsCount];
					for (int step = 0; step < stepsCount; step++)
						movement.steps[step] = _bytes[index++];
				}

				int GetStepsCount() => length - sizeof(int);
			}
			#endregion
		}
	}
}
