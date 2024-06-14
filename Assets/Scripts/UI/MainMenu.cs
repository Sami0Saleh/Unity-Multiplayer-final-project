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
	[SerializeField] private Button _logOutButton;
	[SerializeField] private Button _exitButton;

	private string Nickname { set => _nicknameText.text = value; }

	private void Start()
	{
		_playButton.onClick.AddListener(PlayButton);
		_logOutButton.onClick.AddListener(LogOutButton);
		_exitButton.onClick.AddListener(ExitButton);
	}

	private void OnEnable()
	{
		Nickname = PhotonNetwork.LocalPlayer.NickName;

        ToggleButtonsState(true);
    }

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (targetPlayer == PhotonNetwork.LocalPlayer)
			Nickname = targetPlayer.NickName;
	}

	public void PlayButton()
	{
		PhotonNetwork.JoinLobby(MainMenuManager.Instance.GameLobby); //TODO add lobby name

        ToggleButtonsState(false);
    }

	//TODO add on failed to join lobby method

	public void LogOutButton()
	{
		PhotonNetwork.Disconnect();

        ToggleButtonsState(false);
    }

    public void ExitButton()
	{
		Application.Quit();

        ToggleButtonsState(false);
    }

    public void ToggleButtonsState(bool active)
    {
        _playButton.interactable = active;
		_logOutButton.interactable = active;
        _exitButton.interactable = active;
    }
}
