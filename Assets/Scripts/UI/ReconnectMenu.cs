using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace UI
{
	public class ReconnectMenu : MonoBehaviourPunCallbacks
	{
		[SerializeField] private Button _rejoinButton;

		private void Start()
		{
			_rejoinButton.onClick.AddListener(OnRejoin);
		}

		private void OnRejoin()
		{
			PhotonNetwork.RejoinRoom(MainMenuManager.LastRoomName);
		}
	}
}
