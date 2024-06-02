using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MainMenu : MonoBehaviourPunCallbacks
{
	[SerializeField] private TextMeshProUGUI _nicknameText;
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _LogOutButton;
	[SerializeField] private Button _exitButton;

	private string Nickname { set => _nicknameText.text = value; }

	private void Start()
	{
		_playButton.onClick.AddListener(PlayButton);
		_LogOutButton.onClick.AddListener(LogOutButton);
		_exitButton.onClick.AddListener(ExitButton);
	}

	private void OnEnable()
	{
		Nickname = PhotonNetwork.LocalPlayer.NickName;
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (targetPlayer == PhotonNetwork.LocalPlayer)
			Nickname = targetPlayer.NickName;
	}

	public void PlayButton()
	{
		PhotonNetwork.JoinLobby(MainMenuManager.Instance.GameLobby);
	}

	public void LogOutButton()
	{
		PhotonNetwork.Disconnect();
	}

	public void ExitButton()
	{
		Application.Quit();
	}
}
