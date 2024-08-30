using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using WebSocketSharp;

namespace UI
{
	public class LoginMenu : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _nicknameInput;
		[SerializeField] private Button _connectButton;
		[SerializeField] private Button _exitButton;

		private void Start()
		{
			_nicknameInput.text = Environment.UserName;
			_connectButton.onClick.AddListener(ConnectButton);
			_exitButton.onClick.AddListener(ExitButton);
			_nicknameInput.onSubmit.AddListener(ConnectButton);
		}

		private void OnEnable() => ToggleButtonsState(true);

		public void ConnectButton() => ConnectButton(_nicknameInput.text);

		public void ConnectButton(string nickname)
		{
			if (nickname.IsNullOrEmpty())
				return;
			PhotonNetwork.LocalPlayer.NickName = nickname;
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
}