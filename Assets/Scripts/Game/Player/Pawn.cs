using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	public class Pawn : MonoBehaviourPun
	{
		public InputActions InputActions { get; private set; }
		public Cursor Cursor { get; set; }
		public byte Position { get; private set; }
		public (byte, byte) PositionIndex
		{
			get => Board.BoardMask.BitNumberToIndex(Position);
			set => Position = Board.BoardMask.IndexToBitNumber(value.Item1, value.Item2);
		}
		public bool IsOnBoard => Board.Instance.CurrentBoardState.Contains(Position);
		public static Pawn Mine { get; private set; }

		public static event UnityAction<Pawn> PlayerJoined;
		public static event UnityAction<Pawn> PlayerEliminated;

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
				transform.SetParent(Board.Instance.PlayerParent);
				var board = Board.Instance;
				PositionIndex = board.CellToIndex(board.Grid.WorldToCell(transform.position));
			}
		}

		private void OnDestroy() => InputActions.Dispose();

		private void OnEnable()
		{
			PlayerJoined?.Invoke(this);
			if (photonView.AmController)
				InputActions.Cursor.Enable();
		}

		private void OnDisable()
		{
			PlayerEliminated?.Invoke(this);
			if (photonView.AmController)
				InputActions.Cursor.Disable();
		}
	}
}
