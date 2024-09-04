using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;
		private Board _board;
		private byte _currentPosition;

		[Tooltip("Called whenever the cursor changes the tile on which it points.")] public event UnityAction<byte> PositionChanged;
		[Tooltip("Called once the player picks a tile and clicks it.")] public event UnityAction<byte> PositionPicked;

		public PunPlayer Owner => photonView.Owner;
		public Pawn OwnerPawn => GameManager.Instance.ActivePlayers[Owner];

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
			var input = OwnerPawn.InputActions;
			if (input == null)
				return;
			input.Cursor.Select.started += OnPick;
		}

		private void OnDisable()
		{
			var input = OwnerPawn.InputActions;
			if (input == null)
				return;
			input.Cursor.Select.started -= OnPick;
		}

		private void Start() => _board = Board.Instance;

		private void Update()
		{
			var currentPosition = _board.WorldPositionToBitNumber(transform.position);
			if (_currentPosition != currentPosition)
			{
				_currentPosition = currentPosition;
				PositionChanged?.Invoke(currentPosition);
			}
		}

		private void OnPick(InputAction.CallbackContext context)
		{
			if (!_board.CurrentBoardState.Contains(_currentPosition))
				return;
			PositionPicked?.Invoke(_currentPosition);
		}
	}
}
