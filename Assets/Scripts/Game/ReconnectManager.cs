using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace Game
{
    public class ReconnectManager : MonoBehaviourPunCallbacks
    {
        public string roomName;
        private bool _intentionalDisconnect = false;

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
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void TryRejoinRoom()
        {
            if (!string.IsNullOrEmpty(roomName))
            {
                bool rejoinResult = PhotonNetwork.RejoinRoom(roomName);
                Debug.Log("Rejoin attempt: " + rejoinResult);
            }
            else
            {
                Debug.LogError("Room name is empty. Cannot rejoin.");
            }
        }
        
        private IEnumerator TryReconnectAndRejoin()
        {
            yield return new WaitForSeconds(2f);
            if (!_intentionalDisconnect)
            {
                PhotonNetwork.ReconnectAndRejoin();
            }
        }

        public void SetIntentionalDisconnect(string currentRoomName)
        {
            _intentionalDisconnect = true;
            roomName = currentRoomName;
        }
        public override void OnConnectedToMaster()
        {
            if (!_intentionalDisconnect)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        public override void OnJoinedLobby()
        {
            if (_intentionalDisconnect)
            {
                TryRejoinRoom();
            }
            else
            {
                PhotonNetwork.JoinLobby();
            }
        }
        public override void OnRoomListUpdate(System.Collections.Generic.List<RoomInfo> roomList)
        {
            if (_intentionalDisconnect)
            {
                foreach (RoomInfo room in roomList)
                {
                    if (room.Name == roomName)
                    {
                        Debug.Log("Previous room found: " + roomName);
                        break;
                    }
                }
            }
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Failed to rejoin room: {message}");
            if (!_intentionalDisconnect)
            {
                StartCoroutine(TryReconnectAndRejoin());
            }
        }
        public override void OnJoinedRoom()
        {
            if (_intentionalDisconnect)
            {
                _intentionalDisconnect = false;
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
}
