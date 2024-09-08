using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PunPlayer = Photon.Realtime.Player;

#if UNITY_EDITOR
namespace Test
{
    public class DebugHelper : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject _rejoinButton;
        [SerializeField] private GameObject _transferMasterClientButton;

        public static DebugHelper Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _rejoinButton.SetActive(false);
            _transferMasterClientButton.SetActive(PhotonNetwork.IsMasterClient);
        }

		#region BUTTONS
		public void LeaveRoom()
        {
            var reconnectManager = FindObjectOfType<ReconnectManager>();
            if (reconnectManager != null)
            {
                reconnectManager.SetIntentionalDisconnect(PhotonNetwork.CurrentRoom.Name);
            }

            PhotonNetwork.LeaveRoom();

            _rejoinButton.SetActive(true);
        }

        public void RejoinRoom()
        {
            Debug.Log("Rejoining the room...");
            var reconnectManager = FindObjectOfType<ReconnectManager>();
            if (reconnectManager != null)
                PhotonNetwork.RejoinRoom(reconnectManager.RoomName);

            _rejoinButton.SetActive(false);
        }

        public void TransferMasterClient()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
			PunPlayer candidateMC = PhotonNetwork.LocalPlayer.GetNext();
            bool success = PhotonNetwork.SetMasterClient(candidateMC);
            Debug.Log("Set master client result " + success);
        }
		#endregion

		public override void OnMasterClientSwitched(Player newMasterClient)
		{
			Debug.Log($"New MasterClient: {newMasterClient.NickName}");
			_transferMasterClientButton.SetActive(PhotonNetwork.IsMasterClient);
		}
	}
}
#endif
