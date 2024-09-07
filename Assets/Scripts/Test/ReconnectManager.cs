using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

#if UNITY_EDITOR
namespace Test
{
    public class ReconnectManager : MonoBehaviourPunCallbacks
    {
        public string RoomName { get; private set; }
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
            if (!string.IsNullOrEmpty(RoomName))
            {
                bool rejoinResult = PhotonNetwork.RejoinRoom(RoomName);
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
            RoomName = currentRoomName;
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
                    if (room.Name == RoomName)
                    {
                        Debug.Log("Previous room found: " + RoomName);
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

                var playerObjects = FindObjectsByType<Game.Player.Pawn>(FindObjectsSortMode.None);
                
                foreach (var obj in playerObjects)
                {
                    if (obj.Owner.IsLocal)
                        obj.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
        }
    }
}
#endif
