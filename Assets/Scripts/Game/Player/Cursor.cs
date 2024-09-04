using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;
		private Grid _grid;
		private Vector2Int _lastPosition;

		public event UnityAction<Vector2Int> PositionChanged;

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

		private void Start() => _grid = Board.Instance.Grid;

		private void Update()
		{
			var currentPosition = (Vector2Int)_grid.WorldToCell(transform.position);
			if (currentPosition != _lastPosition)
			{
				_lastPosition = currentPosition;
				PositionChanged?.Invoke(currentPosition);
			}
		}
	}
}
