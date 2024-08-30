using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	public class Pawn : MonoBehaviourPun
	{
		public InputActions InputActions { get; private set; }
		public Cursor Cursor { get; set; }
		public static Pawn Mine { get; private set; }

		public static event UnityAction<Pawn> PlayerJoined;
		public static event UnityAction<Pawn> PlayerEliminated;

		public Photon.Realtime.Player Owner => photonView.Owner;

		private void Awake()
		{
			gameObject.name = Owner.NickName;
			transform.SetParent(Board.Instance.PlayerParent);
			if (!photonView.AmOwner)
				return;
			RegisterMine();
			InputActions = new();
			PlayerJoined?.Invoke(this);

			void RegisterMine()
			{
				if (Mine == null)
					Mine = this;
			}
		}

		private void OnEnable()
		{
			if (photonView.AmOwner)
				InputActions.Cursor.Enable();
		}

		private void OnDisable()
		{
			if (photonView.AmOwner)
				InputActions.Cursor.Disable();
		}
	}
}
