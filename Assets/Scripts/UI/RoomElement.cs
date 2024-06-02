using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roomName;
	[SerializeField] private TextMeshProUGUI _playerCount;
	[SerializeField] private Button _joinButton;

	private void Start()
	{
		_joinButton.onClick.AddListener(JoinButton);
	}

	private void JoinButton()
	{
		PhotonNetwork.JoinRoom(_roomName.text);
	}

	public void SetProperties(RoomInfo roomInfo)
	{
		SetTexts(roomInfo.Name, roomInfo.PlayerCount, roomInfo.MaxPlayers);
	}

	private void SetTexts(string roomName, int playerCount, int maxPlayers)
	{
		_roomName.text = roomName;
		_playerCount.text = $"{playerCount}/{maxPlayers}";
	}
}
