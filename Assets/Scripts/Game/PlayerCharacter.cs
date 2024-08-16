using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
	public class PlayerCharacter : MonoBehaviourPun
	{
		[SerializeField] private PlayerCursor _cursorPrefab;
		public InputActions InputActions { get; private set; }
		public PlayerCursor Cursor { get; private set; }
		public static PlayerCharacter Mine { get; private set; } 

		public static event UnityAction<PlayerCharacter> PlayerJoined;
		public static event UnityAction<PlayerCharacter> PlayerEliminated;

		public Player ThisPlayer => photonView.Owner;

		private void Awake()
		{
			gameObject.name = ThisPlayer.NickName;
			transform.SetParent(Board.Instance.PlayerParent);
			if (!photonView.AmOwner)
				return;
			RegisterMine();
			InputActions = new();
			Cursor = PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation).GetComponent<PlayerCursor>();

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
