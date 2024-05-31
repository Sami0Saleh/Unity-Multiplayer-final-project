using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkedObject : MonoBehaviourPunCallbacks
{
	private const string LOBBY_DEFAULT_NAME = "TheWheel";

    [SerializeField] InputField nameInputField;
	[SerializeField] InputField roomNameInputField;
	[SerializeField] TMPro.TextMeshPro statusText;
	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected");
		statusText.text = ("Successfuly Connected To Servers");
	}

	public void ConnectToPhoton() // Connect Button
	{
		if (nameInputField.text != null)
		{ PhotonNetwork.ConnectUsingSettings(); }
		else
		{ nameInputField.text = "Lior Hadshian"; PhotonNetwork.ConnectUsingSettings(); }
    }

	public void Play() // Play Button
	{
        PhotonNetwork.JoinLobby(new TypedLobby(LOBBY_DEFAULT_NAME, LobbyType.Default));
        // Switch To Room Selection UI
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
		Debug.Log($"Joined Lobby {PhotonNetwork.CurrentLobby}");
    }

    public void Exit() // Exit Button
	{
        Application.Quit();
    }

	public void CreateRoom()
	{
		PhotonNetwork.CreateRoom(roomNameInputField.text);
	}

	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
		Debug.Log("Created Room");
		statusText.text = ("Created Room Successfully");
	}

	public void JoinRandomRoom() // joined A random Room
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom("Whatever Room I Clicked On");
	}
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
		Debug.Log($"joined {PhotonNetwork.CurrentRoom}");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // change UI Based On Room List
	{
        base.OnRoomListUpdate(roomList);
        foreach (RoomInfo roomInfo in roomList)
        {
            // change UI
        }
    }


}
