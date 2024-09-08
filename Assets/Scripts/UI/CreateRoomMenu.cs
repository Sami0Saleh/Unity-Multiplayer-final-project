using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using WebSocketSharp;

namespace UI
{
	public class CreateRoomMenu : MonoBehaviour
	{
		public const int PLAYER_TTL = 60_000;

		[SerializeField] private TMP_InputField _roomName;
		[SerializeField] private Slider _maxPlayerCount;
		[SerializeField] private TextMeshProUGUI _maxPlayerText;
		[SerializeField] private Button _createRoomButton;
		[SerializeField] private Button _backButton;

		private string RoomName => _roomName.text;
		private int MaxPlayerCount => Mathf.Clamp((int)_maxPlayerCount.value, 2, MainMenuManager.MAX_PLAYERS_PER_ROOM);

		private void OnValidate() => _maxPlayerCount.maxValue = MainMenuManager.MAX_PLAYERS_PER_ROOM;

		private void Start()
		{
			_createRoomButton.onClick.AddListener(CreateRoomButton);
			_backButton.onClick.AddListener(BackButton);
			_roomName.text = $"{PhotonNetwork.LocalPlayer.NickName}'s Room";
		}

		private void OnEnable()
		{
			ToggleButtonsState(true);
		}

		public void CreateRoomButton()
		{
			if (RoomName.IsNullOrEmpty())
				return;
			PhotonNetwork.CreateRoom(RoomName, new() { MaxPlayers = MaxPlayerCount, PlayerTtl = PLAYER_TTL, CleanupCacheOnLeave = false });

			ToggleButtonsState(false);
		}

		public void BackButton()
		{
			MainMenuManager.Instance.ExitCreateRoomMenu();

			ToggleButtonsState(false);
		}

		public void UpdateMaxPlayersText(float value)
		{
			_maxPlayerText.text = $"Max Players: {(int)value}";
		}

		public void ToggleButtonsState(bool active)
		{
			_createRoomButton.interactable = active;
			_backButton.interactable = active;
		}
	}
}