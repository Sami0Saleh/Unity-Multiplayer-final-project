using System;
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
		_nickname.text = Environment.UserName;
		_connectButton.onClick.AddListener(ConnectButton);
		_exitButton.onClick.AddListener(ExitButton);
	}

    private void OnEnable()
    {
        ToggleButtonsState(true);
    }

    public void ConnectButton()
	{
		if (Nickname.IsNullOrEmpty())
			return;
		PhotonNetwork.LocalPlayer.NickName = Nickname;
		PhotonNetwork.ConnectUsingSettings();

        ToggleButtonsState(false);
    }

    public void ExitButton()
	{
		Application.Quit();

        ToggleButtonsState(false);
    }

	public void ToggleButtonsState(bool active)
	{
		_connectButton.interactable = active;
		_exitButton.interactable = active;
	}
}
