using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace UI
{
	public class RoomElement : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _roomName;
		[SerializeField] private TextMeshProUGUI _playerCount;
		[SerializeField] private Image _isOpen;
		[SerializeField] private Button _joinButton;

		public RoomInfo RoomInfo { get; private set; }

		private void Start()
		{
			_joinButton.onClick.AddListener(JoinButton);
		}

		private void OnEnable()
		{
			ToggleButtonsState(true);
		}

		private void JoinButton()
		{
			PhotonNetwork.JoinRoom(_roomName.text);
			ToggleButtonsState(false);
		}

		public void SetProperties(RoomInfo roomInfo)
		{
			RoomInfo = roomInfo;
			SetTexts(roomInfo.Name, roomInfo.PlayerCount, roomInfo.MaxPlayers);
			_isOpen.enabled = roomInfo.IsOpen;
		}

		private void SetTexts(string roomName, int playerCount, int maxPlayers)
		{
			_roomName.text = roomName;
			_playerCount.text = $"{playerCount}/{maxPlayers}";
		}

		public void ToggleButtonsState(bool active)
		{
			_joinButton.interactable = active;
		}
	}
}