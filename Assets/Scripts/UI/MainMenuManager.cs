using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Game.Player.Visuals;

namespace UI
{
	public class MainMenuManager : MonoBehaviourPunCallbacks
	{
		public const int MAX_PLAYERS_PER_ROOM = 4;
		public static string LastRoomName { get; private set; }

		[SerializeField] private ErrorPopup _errorPopup;

		[field: SerializeField] public LoginMenu LoginMenu { get; private set; }
		[field: SerializeField] public MainMenu MainMenu { get; private set; }
		[field: SerializeField] public JoinRoomMenu JoinRoomMenu { get; private set; }
		[field: SerializeField] public CreateRoomMenu CreateRoomMenu { get; private set; }
		[field: SerializeField] public RoomMenu RoomMenu { get; private set; }
		private IEnumerable<MonoBehaviour> Menus
		{
			get
			{
				yield return LoginMenu;
				yield return MainMenu;
				yield return JoinRoomMenu;
				yield return CreateRoomMenu;
				yield return RoomMenu;
			}
		}
		public static MainMenuManager Instance { get; private set; }
		public TypedLobby DefaultLobby { get; private set; }

		private void OnValidate()
		{
			SetActiveMenu(LoginMenu);
		}

		private void Awake()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
			if (Instance == null)
				Instance = this;
			DefaultLobby = DefaultLobby = new("Default Lobby", LobbyType.Default);
			if (!PhotonNetwork.LocalPlayer.CustomProperties.HasColorProperty())
				PhotonNetwork.LocalPlayer.SetColorProperty(PlayerColors.DEFAULT_COLOR);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			CheckNetworkState();

			void CheckNetworkState()
			{
				if (PhotonNetwork.InRoom)
					SetActiveMenu(RoomMenu);
				else if (PhotonNetwork.InLobby)
					SetActiveMenu(JoinRoomMenu);
				else if (PhotonNetwork.IsConnected)
					SetActiveMenu(MainMenu);
			}
		}

		private void SetActiveMenu([DisallowNull] MonoBehaviour menuToActivate)
		{
			foreach (var menu in Menus)
				menu.gameObject.SetActive(menu == menuToActivate);
		}

		public override void OnConnectedToMaster()
		{
			if (!PhotonNetwork.InLobby)
				SetActiveMenu(MainMenu);
			else
				SetActiveMenu(JoinRoomMenu);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			SetActiveMenu(LoginMenu);
			Debug.Log("Disconnected from server");
			if (cause != DisconnectCause.DisconnectByClientLogic)
				PopUpErrorMessage(cause.ToString());
		}

		public void JoinLobby()
		{
			PhotonNetwork.JoinLobby(DefaultLobby);
		}

		public override void OnJoinedLobby()
		{
			if (PhotonNetwork.CurrentLobby == DefaultLobby)
				SetActiveMenu(JoinRoomMenu);
		}

		public override void OnLeftLobby()
		{
			if (!PhotonNetwork.InLobby)
				SetActiveMenu(MainMenu);
		}

		public void ToCreateRoomMenu()
		{
			SetActiveMenu(CreateRoomMenu);
		}

		public void ExitCreateRoomMenu()
		{
			SetActiveMenu(JoinRoomMenu);
		}

		public override void OnJoinedRoom()
		{
			LastRoomName = PhotonNetwork.CurrentRoom.Name;
			SetActiveMenu(RoomMenu);
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

		public void ToggleButtonState(bool active)
		{
			MainMenu.ToggleButtonsState(active); //Could make it only apply to the active room, but idk how to do it rn, maybe later when I have time
			LoginMenu.ToggleButtonsState(active);
			JoinRoomMenu.ToggleButtonsState(active);
			CreateRoomMenu.ToggleButtonsState(active);
		}
	}
}