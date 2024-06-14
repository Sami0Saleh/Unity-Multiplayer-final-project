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
	
	public RoomInfo RoomInfo { get; private set; }

	private void Start()
	{
		_joinButton.onClick.AddListener(JoinButton);
	}

    private void OnEnable()
    {
        _joinButton.interactable = true;
    }

    private void JoinButton()
	{
		PhotonNetwork.JoinRoom(_roomName.text);
		_joinButton.interactable = false;
	}

	public void SetProperties(RoomInfo roomInfo)
	{
		RoomInfo = roomInfo;
		SetTexts(roomInfo.Name, roomInfo.PlayerCount, roomInfo.MaxPlayers);
	}

	private void SetTexts(string roomName, int playerCount, int maxPlayers)
	{
		_roomName.text = roomName;
		_playerCount.text = $"{playerCount}/{maxPlayers}";
	}
}
