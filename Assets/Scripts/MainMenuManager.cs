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

    [SerializeField] private ErrorPopup _errorPopup;

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
		Debug.Log("Disconnected from server");
		if (cause != DisconnectCause.DisconnectByClientLogic)
			PopUpErrorMessage(cause.ToString());
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
			Debug.Log($"Joined Lobby {PhotonNetwork.CurrentLobby}");
			return;
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

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Create Room");
        PopUpErrorMessage(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Join Room");
		PopUpErrorMessage(message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Join Random Room");
        PopUpErrorMessage(message);
    }

    public void PopUpErrorMessage(string error)
	{
		_errorPopup.EditErrorText(error);
		_errorPopup.gameObject.SetActive(true);
	}

	public void ResolveErrorPopup()
	{
		_errorPopup.gameObject.SetActive(false);
		_mainMenu.ToggleButtonsState(true); //Could make it only apply to the active room, but idk how to do it rn, maybe later when I have time
		_loginMenu.ToggleButtonsState(true);
        _joinRoomMenu.ToggleButtonsState(true);
		_createRoomMenu.ToggleButtonsState(true);
    }
}
