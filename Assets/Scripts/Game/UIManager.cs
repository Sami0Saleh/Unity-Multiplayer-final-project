using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;


namespace Game
{
    public class UIManager : MonoBehaviour
    {
        private const int MENU_SCENE_INDEX = 0;

        [SerializeField] private TextMeshProUGUI _uiMessageText;
        [SerializeField] private GameObject _rejoinButton;
        [SerializeField] private GameObject _transferMasterClientButton;

        public static UIManager Instance;

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

        public void UpdateText(string text)
        {
            _uiMessageText.text = text;
            StartCoroutine(EmptyText());
        }

        public void LeaveRoom()
        {
            var reconnectManager = FindObjectOfType<ReconnectManager>();
            if (reconnectManager != null)
            {
                reconnectManager.SetIntentionalDisconnect(PhotonNetwork.CurrentRoom.Name);
            }

            PhotonNetwork.LeaveRoom(true);

            _rejoinButton.SetActive(true);
        }

        public void RejoinRoom()
        {
            UpdateText("Rejoining the room...");
            var reconnectManager = FindObjectOfType<ReconnectManager>();
            if (reconnectManager != null)
            {
                PhotonNetwork.RejoinRoom(reconnectManager.roomName);
            }

            _rejoinButton.SetActive(false);
        }

        public void TransferMasterClient()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Player candidateMC = PhotonNetwork.LocalPlayer.GetNext();

                bool success = PhotonNetwork.SetMasterClient(candidateMC);
                Debug.Log("set master client result " + success);

            }
            
        }
        public void UpdateMasterClientButtonState(bool isActive)
        {
            _transferMasterClientButton.SetActive(isActive);
        }
        private IEnumerator EmptyText()
        {
            yield return new WaitForSeconds(2f);
            _uiMessageText.text = null;
        }
    }
}
