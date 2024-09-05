using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	/// <summary>
	/// Handles <see cref="TurnIterator.TurnChangeEvent.Action.Hammer"/> events.
	/// </summary>
	public class Hammer : MonoBehaviourPun
	{
		public const int MAX_RANGE = 4;

		[SerializeField] private Pawn _pawn;

		public event UnityAction<byte> OnTileDestroyed;

		private void Awake() => enabled = photonView.AmController;

		private void OnEnable() => TurnIterator.Instance.OnTurnChange += OnTurnChange;

		private void OnDisable() => TurnIterator.Instance.OnTurnChange -= OnTurnChange;

		private void OnTurnChange(TurnIterator.TurnChangeEvent turnChangeEvent)
		{
			if (!turnChangeEvent.Valid)
				return;
			if (turnChangeEvent.currentPlayer.IsLocal && turnChangeEvent.action == TurnIterator.TurnChangeEvent.Action.Hammer)
				ListenToCursorEvents(_pawn.Cursor);
			else
				UnListenToCursorEvents(_pawn.Cursor);

			void ListenToCursorEvents(Cursor cursor) => cursor.PositionPicked += OnPositionPicked;

			void UnListenToCursorEvents(Cursor cursor) => cursor.PositionPicked -= OnPositionPicked;
		}

		private void OnPositionPicked(byte position)
		{
			const string TILE_HAMMERED = nameof(OnHammerRPC);
			photonView.RPC(TILE_HAMMERED, RpcTarget.All, position);
		}

		[PunRPC]
		private void OnHammerRPC(byte tile, PhotonMessageInfo info)
		{
			if (!CurrentlyActingPlayer() || !TileIsOnBoard() || !WithinRangeOfPawn())
				return;
			OnTileDestroyed?.Invoke(tile);

			bool CurrentlyActingPlayer() => TurnIterator.Instance.Current == info.Sender;

			bool TileIsOnBoard() => Board.Instance.CurrentBoardState.Contains(tile);

			bool WithinRangeOfPawn() => Pathfinding.GetArea(_pawn.Position, MAX_RANGE).Contains(tile);
		}
	}
}
