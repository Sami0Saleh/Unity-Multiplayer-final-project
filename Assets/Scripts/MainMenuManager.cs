using System.Linq;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using NUnit.Framework;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
	public const int MAX_PLAYERS_PER_ROOM = 8;

	[SerializeField] private LoginMenu _loginMenu;
	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private JoinRoomMenu _joinRoomMenu;
	[SerializeField] private CreateRoomMenu _createRoomMenu;
	[SerializeField] private RoomMenu _roomMenu;
	[SerializeField, HideInInspector] private GameObject[] _menus;

	public static MainMenuManager Instance { get; private set; }

	public TypedLobby GameLobby { get; private set; }
	public List<TypedLobby> ClientLobbies { get; private set; }

    private void OnValidate()
	{
		_menus = new GameObject[] {
			_loginMenu.gameObject,
			_mainMenu.gameObject,
			_joinRoomMenu.gameObject,
			_createRoomMenu.gameObject,
			_roomMenu.gameObject};
		SetActiveMenu(_loginMenu);
		ClientLobbies = new List<TypedLobby>();
	}

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		GameLobby = GameLobby = new("Main Lobby", LobbyType.Default);
	}

	private void SetActiveMenu([DisallowNull] MonoBehaviour menuToActivate)
	{
        foreach (var menu in _menus.Where(go => go != menuToActivate.gameObject))
            menu.SetActive(false);
		menuToActivate.gameObject.SetActive(true);
    }

	public override void OnConnectedToMaster()
	{
		if (!PhotonNetwork.InLobby)
			SetActiveMenu(_mainMenu);
		else
			SetActiveMenu(_joinRoomMenu);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		SetActiveMenu(_loginMenu);
	}

	public void JoinLobbyByName(string lobbyName)
	{
		TypedLobby typedLobby = null;
		foreach (TypedLobby lobby in ClientLobbies)
		{
			if (lobby.Name == lobbyName)
			{
				typedLobby = lobby;
			}	
		}
		if (typedLobby == null)
		{
            typedLobby = new TypedLobby(lobbyName, LobbyType.Default);
			ClientLobbies.Add(typedLobby);
        }
		PhotonNetwork.JoinLobby(typedLobby);
	}

    public override void OnJoinedLobby()
    {
		if (PhotonNetwork.CurrentLobby == GameLobby) //Checks if player is in default lobby
		{
            SetActiveMenu(_joinRoomMenu);
			Debug.Log("Joined Main Lobby");
			return;
        }

        foreach (TypedLobby lobby in ClientLobbies) //Checks if player is in any client lobby. This is not a good way of doing it, but it'll work for now
		{
			if (PhotonNetwork.CurrentLobby == lobby)
			{
                SetActiveMenu(_joinRoomMenu);
                Debug.Log("Joined client lobby: " + PhotonNetwork.CurrentLobby.Name);
                return;
            }
        }
    }

	public override void OnLeftLobby()
	{
		if (!PhotonNetwork.InLobby)
			SetActiveMenu(_mainMenu);
	}

	public void ToCreateRoomMenu()
	{
		SetActiveMenu(_createRoomMenu);
	}

	public void ExitCreateRoomMenu()
	{
		SetActiveMenu(_joinRoomMenu);
	}

    public override void OnJoinedRoom()
    {
		SetActiveMenu(_roomMenu);
    }

    //TODO add failed to log in
    //TODO add failed to connect to lobby
    //TODO add failed to create room
    //TODO add failed to join room

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Join Room");
        _joinRoomMenu.ToggleButtonsState(true);
    }
}
