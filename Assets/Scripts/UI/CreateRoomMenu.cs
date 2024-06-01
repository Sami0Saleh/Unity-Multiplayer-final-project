using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using WebSocketSharp;

public class CreateRoomMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField _roomName;
	[SerializeField] private Slider _maxPlayerCount;
	[SerializeField] private TextMeshProUGUI _maxPlayerText;
	[SerializeField] private Button _createRoomButton;
	[SerializeField] private Button _backButton;

	private string RoomName => _roomName.text;
	private int MaxPlayerCount => Mathf.Clamp((int)_maxPlayerCount.value, 2, MainMenuManager.MAX_PLAYERS_PER_ROOM);

	private void Start()
	{
		_createRoomButton.onClick.AddListener(CreateRoomButton);
		_backButton.onClick.AddListener(BackButton);
	}

	public void CreateRoomButton()
	{
		if (RoomName.IsNullOrEmpty())
			return;
		PhotonNetwork.CreateRoom(RoomName, new() {MaxPlayers = MaxPlayerCount});
	}

	public void BackButton()
	{
		MainMenuManager.Instance.ExitCreateRoomMenu();
	}

	public void UpdateMaxPlayersText(float value)
	{
		_maxPlayerText.text = $"Max Players: {(int)value}";
	}
}
