using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	public class Pawn : MonoBehaviourPun
	{
		public InputActions InputActions { get; private set; }
		public Cursor Cursor { get; set; }
		[field: SerializeField] public PawnMovement Movement { get; private set; }
		[field: SerializeField] public Hammer Hammer { get; private set; }
		public byte Position { get; set; }
		public (byte, byte) PositionIndex
		{
			get => Board.BoardMask.BitNumberToIndex(Position);
			set => Position = Board.BoardMask.IndexToBitNumber(value.Item1, value.Item2);
		}
		public bool IsOnBoard => Board.Instance.CurrentBoardState.Contains(Position);
		public static Pawn Mine { get; private set; }

		public static event UnityAction<Pawn> PlayerJoined;
		public static event UnityAction<Pawn> PlayerEliminated;
		public event UnityAction<Pawn> MoveTurnStart;
		public event UnityAction<Pawn> HammerTurnStart;
		public event UnityAction<Pawn> TurnEnd;

		public Photon.Realtime.Player Owner => photonView.Owner;

		private void Awake()
		{
			gameObject.name = Owner.NickName;
			SetPosition();
			if (!photonView.AmOwner)
				return;
			RegisterMine();
			InputActions = new();

			void RegisterMine()
			{
				if (Mine == null)
					Mine = this;
			}

			void SetPosition()
			{
				transform.SetParent(Board.Instance.PawnsParent);
				var board = Board.Instance;
				PositionIndex = Board.CellToIndex(board.Grid.WorldToCell(transform.position));
			}
		}

		private void OnDestroy() => InputActions?.Dispose();

		private void OnEnable()
		{
			PlayerJoined?.Invoke(this);
			TurnIterator.Instance.OnTurnChange += OnTurnChange;
			Movement.OnPawnMoved += OnPawnMoved;
			if (photonView.AmController)
				InputActions.Cursor.Enable();
		}

		private void OnDisable()
		{
			PlayerEliminated?.Invoke(this);
			TurnIterator.Instance.OnTurnChange -= OnTurnChange;
			Movement.OnPawnMoved -= OnPawnMoved;
			if (photonView.AmController)
				InputActions.Cursor.Disable();
		}

		private void OnTurnChange(TurnIterator.TurnChangeEvent turnChangeEvent)
		{
			if (turnChangeEvent.currentPlayer == Owner && turnChangeEvent.action == TurnIterator.TurnChangeEvent.Action.Move)
				MoveTurnStart?.Invoke(this);
			else if (turnChangeEvent.currentPlayer == Owner && turnChangeEvent.action == TurnIterator.TurnChangeEvent.Action.Hammer)
				HammerTurnStart?.Invoke(this);
			else if (turnChangeEvent.lastPlayer == Owner)
				TurnEnd?.Invoke(this);
		}

		private void OnPawnMoved(PawnMovement.PawnMovementEvent movementEvent)
		{
			if (movementEvent.player != Owner)
				return;
			Position = movementEvent.steps.Last();
			var tileTransform = Board.Instance.BitNumberToTile(Position).transform;
			transform.SetPositionAndRotation(tileTransform.position, tileTransform.rotation);
		}
	}
}
