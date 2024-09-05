using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	/// <summary>
	/// Handles tile destruction events.
	/// </summary>
	public class Hammer : MonoBehaviourPun
	{
		public const int MAX_RANGE = 4;

		[SerializeField] private Pawn _pawn;

		public event UnityAction<byte> OnHammered;

		public bool AbleToHammer { get; private set; }

		private void Awake() => _pawn.TurnStart += OnStartTurn;

		private void OnDestroy() => _pawn.TurnStart -= OnStartTurn;

		private void OnEnable() => _pawn.Cursor.PositionPicked += OnPositionPicked;

		private void OnDisable() => _pawn.Cursor.PositionPicked -= OnPositionPicked;

		private void OnStartTurn(Pawn pawn) => AbleToHammer = _pawn.IsOnBoard;

		private void OnPositionPicked(byte position)
		{
			const string TILE_HAMMERED = nameof(HammerTileRPC);
			photonView.RPC(TILE_HAMMERED, RpcTarget.All, position);
		}

		[PunRPC]
		private void HammerTileRPC(byte tile, PhotonMessageInfo info)
		{
			if (!AbleToHammer || !CurrentlyActingPlayer() || !TileIsOnBoard() || !WithinRangeOfPawn())
				return;
			AbleToHammer = false;
			OnHammered?.Invoke(tile);

			bool CurrentlyActingPlayer() => TurnIterator.Instance.Current == info.Sender;

			bool TileIsOnBoard() => Board.Instance.CurrentBoardState.Contains(tile);

			bool WithinRangeOfPawn() => Pathfinding.GetArea(_pawn.Position, MAX_RANGE).Contains(tile);
		}
	}
}
