using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using WebSocketSharp;

public class LoginMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField _nickname;
	[SerializeField] private Button _connectButton;
	[SerializeField] private Button _exitButton;

	public string Nickname => _nickname.text;

	private void Start()
	{
		_connectButton.onClick.AddListener(ConnectButton);
		_exitButton.onClick.AddListener(ExitButton);
	}

	public void ConnectButton()
	{
		if (Nickname.IsNullOrEmpty())
			return;
		PhotonNetwork.LocalPlayer.NickName = Nickname;
		PhotonNetwork.ConnectUsingSettings();
	}

	public void ExitButton()
	{
		Application.Quit();
	}
}
