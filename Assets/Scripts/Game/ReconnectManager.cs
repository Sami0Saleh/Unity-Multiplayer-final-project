using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace Game
{
    public class ReconnectManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string roomName;

        private void Start()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                if (PhotonNetwork.InLobby)
                {
                    TryRejoinRoom();
                }
                else
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(); // Ensure connection to Photon
            }
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            TryRejoinRoom();
        }

        private void TryRejoinRoom()
        {
            if (PhotonNetwork.RejoinRoom(roomName))
            {
                Debug.Log("Rejoining room...");
            }
            else
            {
                Debug.LogError("RejoinRoom failed.");
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Failed to rejoin room: {message}");
            // Attempt to reconnect and rejoin the room
            StartCoroutine(TryReconnectAndRejoin());
        }

        private IEnumerator TryReconnectAndRejoin()
        {
            yield return new WaitForSeconds(2f);
            PhotonNetwork.ReconnectAndRejoin();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Successfully rejoined the room.");

            var playerObjects = FindObjectsOfType<PlayerCharacter>();
            foreach (var obj in playerObjects)
            {
                if (obj.ThisPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    obj.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
        }
    }
}
