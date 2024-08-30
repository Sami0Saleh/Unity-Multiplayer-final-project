using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Game.Player
{
	public class Pawn : MonoBehaviourPun
	{
		[SerializeField] private Cursor _cursorPrefab;
		public InputActions InputActions { get; private set; }
		public Cursor Cursor { get; private set; }
		public static Pawn Mine { get; private set; }

		public static event UnityAction<Pawn> PlayerJoined;
		public static event UnityAction<Pawn> PlayerEliminated;

		public Photon.Realtime.Player ThisPlayer => photonView.Owner;

		private void Awake()
		{
			gameObject.name = ThisPlayer.NickName;
			transform.SetParent(Board.Instance.PlayerParent);
			if (!photonView.AmOwner)
				return;
			RegisterMine();
			InputActions = new();
			Cursor = PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation).GetComponent<Cursor>();

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

		private void Start()
		{
			PlayerJoined?.Invoke(this);
		}
	}
}
