using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using static Game.Board.BoardMask;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		#region VARIABLES_AND_PROPERTIES
		[SerializeField] private MousePositionTracker _mousePositionTracker;
		private Board _board;
		private Vector3Int _currentCell;

		[Tooltip("Called whenever the cursor changes the tile on which it points.")] public event UnityAction<Position> PositionChanged;
		[Tooltip("Called once the player picks a tile and clicks it.")] public event UnityAction<Position> PositionPicked;
		[Tooltip("Called once the player changes the state of the cursor.")] public event UnityAction<State> StateChanged;

		public State CurrentState { get; private set; } = State.Move;
		public PunPlayer Owner => photonView.Owner;
		public Pawn OwnerPawn { get; private set; }
		#endregion

		#region FUNCTIONS
		#region UNITY
		private void Awake()
		{
			OwnerPawn = GameManager.Instance.ActivePlayers[Owner];
			OwnerPawn.Cursor = this;
			gameObject.name = $"{Owner.NickName}'s Cursor";
			if (!photonView.AmOwner)
				Destroy(_mousePositionTracker);
			if (!photonView.AmController)
				enabled = false;
			OwnerPawn.TurnStart += OnTurnStart;
			OwnerPawn.TurnEnd += OnTurnEnd;
			OwnerPawn.Movement.OnPawnMoved += OnMoved;
			OwnerPawn.Hammer.OnHammered += OnHammered;
		}

		private void OnDestroy()
		{
			OwnerPawn.TurnStart -= OnTurnStart;
			OwnerPawn.TurnEnd -= OnTurnEnd;
			OwnerPawn.Movement.OnPawnMoved -= OnMoved;
			OwnerPawn.Hammer.OnHammered -= OnHammered;
		}

		private void OnEnable()
		{
			var input = OwnerPawn.InputActions;
			if (input == null)
				return;
			input.Cursor.Select.started += OnPick;
			input.Cursor.ToggleState.started += OnToggleState;
		}

		private void OnDisable()
		{
			var input = OwnerPawn.InputActions;
			if (input == null)
				return;
			input.Cursor.Select.started -= OnPick;
			input.Cursor.ToggleState.started -= OnToggleState;
		}

		private void Start() => _board = Board.Instance;

		private void Update()
		{
			var currentCell = GetCurrentCell();
			if (_currentCell != currentCell && _board.IsCellOnBoard(currentCell))
				PositionChanged?.Invoke(Board.CellToPosition(_currentCell = currentCell));
		}
		#endregion

		#region INPUT
		private void OnPick(InputAction.CallbackContext _)
		{
			var currentCell = GetCurrentCell();
			if (_board.IsCellOnBoard(currentCell))
				PositionPicked?.Invoke(Board.CellToPosition(_currentCell = currentCell));
		}

		private void OnToggleState(InputAction.CallbackContext _)
		{
			if (TryToggle(out var newState))
			{
				const string TOGGLE_STATE = nameof(ToggleStateRPC);
				photonView.RPC(TOGGLE_STATE, RpcTarget.All, newState);
			}

			bool TryToggle(out State newState)
			{
				newState = State.Neutral;
				var ownerPawn = OwnerPawn;
				if (CanSwitchToHammer())
					newState = State.Hammer;
				else if (CanSwitchToMove())
					newState = State.Move;
				return newState != State.Neutral;

				bool CanSwitchToHammer() => CurrentState == State.Move && ownerPawn.Hammer.AbleToHammer;

				bool CanSwitchToMove() => CurrentState == State.Hammer && ownerPawn.Movement.AbleToMove;
			}
		}
		#endregion

		#region CURSOR
		private void OnTurnStart(Pawn pawn)
		{
			enabled = true;
			ChangeStateLocal(pawn.Movement.AbleToMove ? State.Move : State.Hammer, pawn);
		}

		private void OnTurnEnd(Pawn pawn)
		{
			enabled = false;
			ChangeStateLocal(State.Neutral, pawn);
		}

		private void OnMoved(PawnMovement.PawnMovementEvent _)
		{
			if (OwnerPawn.Movement.AbleToMove)
				return;
			else if (OwnerPawn.Hammer.AbleToHammer)
				ChangeStateLocal(State.Hammer);
		}

		private void OnHammered(Position _)
		{
			if (OwnerPawn.Hammer.AbleToHammer)
				return;
			else if (OwnerPawn.Movement.AbleToMove)
				ChangeStateLocal(State.Move);
		}

		private void ChangeStateLocal(State newState) => ChangeStateLocal(newState, OwnerPawn);

		private void ChangeStateLocal(State newState, Pawn pawn)
		{
			CurrentState = newState;
			StateChanged?.Invoke(CurrentState);
			if (!photonView.AmController)
				return;
			pawn.Movement.enabled = newState == State.Move;
			pawn.Hammer.enabled = newState == State.Hammer;
		}

		[PunRPC]
		private void ToggleStateRPC(State state) => ChangeStateLocal(state);

		private Vector3Int GetCurrentCell() => _board.Grid.WorldToCell(transform.position);
		#endregion
		#endregion

		public enum State : byte
		{
			Neutral,
			Move,
			Hammer
		}
	}
}
