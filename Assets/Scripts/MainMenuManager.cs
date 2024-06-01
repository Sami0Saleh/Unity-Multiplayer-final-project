using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics.CodeAnalysis;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
	public const int PLAYERS_PER_ROOM = 4;

	[SerializeField] private LoginMenu _loginMenu;
	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private JoinRoomMenu _joinRoomMenu;
	[SerializeField] private CreateRoomMenu _createRoomMenu;
	[SerializeField, HideInInspector] private GameObject[] _menus;

	public static MainMenuManager Instance { get; private set; }

	public TypedLobby GameLobby { get; private set; }

	private void OnValidate()
	{
		_menus = new GameObject[] {
			_loginMenu.gameObject,
			_mainMenu.gameObject,
			_joinRoomMenu.gameObject,
			_createRoomMenu.gameObject };
		SetActiveMenu(_loginMenu);
	}

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		GameLobby = GameLobby = new("Game Lobby", LobbyType.Default);
	}

	private void SetActiveMenu([DisallowNull] MonoBehaviour menuToActivate)
	{
        foreach (var menu in _menus)
            menu.SetActive(false);
		menuToActivate.gameObject.SetActive(true);
    }

	public override void OnConnectedToMaster()
	{
		SetActiveMenu(_mainMenu);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		SetActiveMenu(_loginMenu);
	}

    public override void OnJoinedLobby()
    {
		if (PhotonNetwork.CurrentLobby == GameLobby)
			SetActiveMenu(_joinRoomMenu);
    }

	public void ToCreateRoomMenu()
	{
		SetActiveMenu(_createRoomMenu);
	}

	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
		Debug.Log("Created Room");
		//statusText.text = ("Created Room Successfully");
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
