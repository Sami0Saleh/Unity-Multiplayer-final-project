using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WebSocketSharp;
using Photon.Pun;

public class CreateRoomMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField _roomName;
	[SerializeField] private Button _createRoomButton;
	[SerializeField] private Button _backButton;

	private string RoomName => _roomName.text;

	private void Start()
	{
		_createRoomButton.onClick.AddListener(CreateRoomButton);
		_backButton.onClick.AddListener(BackButton);
	}

	public void CreateRoomButton()
	{
		if (RoomName.IsNullOrEmpty())
			return;
		PhotonNetwork.CreateRoom(RoomName);
	}

	public void BackButton()
	{
		MainMenuManager.Instance.ExitCreateRoomMenu();
	}
}
