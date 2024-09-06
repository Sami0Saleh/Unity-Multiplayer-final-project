using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using static Game.Board.BoardMask;

namespace Game.Player
{
	/// <summary>
	/// Handles tile destruction events.
	/// </summary>
	public class Hammer : MonoBehaviourPun
	{
		public const int MAX_RANGE = 4;

		[SerializeField] private Pawn _pawn;

		public event UnityAction<Position> OnHammered;

		public bool AbleToHammer { get; private set; }
		public Board.BoardMask HammerableArea => Board.Instance.CurrentBoardState & Pathfinding.GetArea(_pawn.Position, MAX_RANGE);

		private void Awake() => _pawn.TurnStart += OnStartTurn;

		private void OnDestroy() => _pawn.TurnStart -= OnStartTurn;

		private void OnEnable() => _pawn.Cursor.PositionPicked += OnPositionPicked;

		private void OnDisable() => _pawn.Cursor.PositionPicked -= OnPositionPicked;

		private void OnStartTurn(Pawn pawn) => AbleToHammer = _pawn.IsOnBoard;

		private void OnPositionPicked(Position position)
		{
			const string TILE_HAMMERED = nameof(HammerTileRPC);
			photonView.RPC(TILE_HAMMERED, RpcTarget.All, (byte)position);
		}

		[PunRPC]
		private void HammerTileRPC(byte position, PhotonMessageInfo info)
		{
			if (!AbleToHammer || !CurrentlyActingPlayer() || !WithinHammerableArea())
				return;
			AbleToHammer = false;
			OnHammered?.Invoke(position);

			bool CurrentlyActingPlayer() => TurnIterator.Instance.Current == info.Sender;

			bool WithinHammerableArea() => HammerableArea.Contains((Position)position);
		}
	}
}
