using TMPro;
using UnityEngine;
using Photon.Realtime;

public class RoomElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roomName;
	[SerializeField] private TextMeshProUGUI _playerCount;

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
