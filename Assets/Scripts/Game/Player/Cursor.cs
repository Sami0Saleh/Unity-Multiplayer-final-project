﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		#region VARIABLES_AND_PROPERTIES
		[SerializeField] private MousePositionTracker _mousePositionTracker;
		private Board _board;
		private Vector3Int _currentCell;

		[Tooltip("Called whenever the cursor changes the tile on which it points.")] public event UnityAction<byte> PositionChanged;
		[Tooltip("Called once the player picks a tile and clicks it.")] public event UnityAction<byte> PositionPicked;
		[Tooltip("Called once the player changes the state of the cursor.")] public event UnityAction<State> StateChanged;

		public State CurrentState { get; private set; } = State.Move;
		public PunPlayer Owner => photonView.Owner;
		public Pawn OwnerPawn => GameManager.Instance.ActivePlayers[Owner];
		#endregion

		#region FUNCTIONS
		#region UNITY
		private void Awake()
		{
			OwnerPawn.Cursor = this;
			gameObject.name = $"{Owner.NickName}'s Cursor";
			if (!photonView.AmOwner)
				Destroy(_mousePositionTracker);
			if (!photonView.AmController)
				enabled = false;
		}

		private void OnEnable()
		{
			OwnerPawn.TurnStart += OnTurnStart;
			OwnerPawn.TurnEnd += OnTurnEnd;
			var input = OwnerPawn.InputActions;
			if (input == null)
				return;
			input.Cursor.Select.started += OnPick;
			input.Cursor.ToggleState.started += OnToggleState;
		}

		private void OnDisable()
		{
			OwnerPawn.TurnStart -= OnTurnStart;
			OwnerPawn.TurnEnd -= OnTurnEnd;
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
				PositionChanged?.Invoke(Board.CellToBitNumber(_currentCell = currentCell));
		}
		#endregion

		#region INPUT
		private void OnPick(InputAction.CallbackContext _)
		{
			var currentCell = GetCurrentCell();
			if (_board.IsCellOnBoard(currentCell))
				PositionPicked?.Invoke(Board.CellToBitNumber(_currentCell = currentCell));
		}

		private void OnToggleState(InputAction.CallbackContext _)
		{
			CurrentState = CurrentState == State.Move ? State.Hammer : State.Move;
			const string TOGGLE_STATE = nameof(ToggleStateRPC);
			photonView.RPC(TOGGLE_STATE, RpcTarget.All, CurrentState);
		}
		#endregion

		#region CURSOR
		private void OnTurnStart(Pawn pawn) => ChangeStateLocal(pawn.Movement.AbleToMove ? State.Move : State.Hammer, pawn);

		private void OnTurnEnd(Pawn pawn) => ChangeStateLocal(State.Neutral, pawn);

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
