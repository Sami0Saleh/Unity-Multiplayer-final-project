using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerElement : MonoBehaviourPunCallbacks
{
	const string REQUEST_COLOR = nameof(RequestColor);
	const string SET_TAKEN_POPUP = nameof(SetTakenPopup);

	[SerializeField] private PlayerColors _colorConfig;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _leftArrow;
    [SerializeField] private TextMeshProUGUI _colorTextbox;
	[SerializeField] private TextMeshProUGUI _takenPopup;
    private int _currentColorIndex = 0;

    public Player ThisPlayer => photonView.Owner;

	private void Start()
    {
        AddToPlayerGroup();
		SetColorText(_colorConfig[_currentColorIndex]);
		SetNameText(ThisPlayer.NickName);

		if (!photonView.IsMine)
            return;

        BindButtons();
        ThisPlayer.SetColorProperty(_colorConfig[_currentColorIndex]);

        void BindButtons()
        {
			_rightArrow.onClick.AddListener(CycleColorRightButton);
			_leftArrow.onClick.AddListener(CycleColorLeftButton);
			_rightArrow.gameObject.SetActive(photonView.IsMine);
			_leftArrow.gameObject.SetActive(photonView.IsMine);
		}

        void AddToPlayerGroup()
		{
			transform.SetParent(MainMenuManager.Instance.RoomMenu.PlayerList);
			transform.localScale = Vector3.one;
		}
	}

	private void SetNameText(string playerName) => _playerName.text = playerName;

    private void SetColorText(string color) => _colorTextbox.text = color;

	private void CycleColorRightButton() => CycleColor(1);

	private void CycleColorLeftButton() => CycleColor(-1);

	private void CycleColor(int direction)
    {
        Debug.Assert(photonView.IsMine, "Cannot cycle colors on other player's PlayerElement.");
        if (_currentColorIndex + direction < 0)
        {
            _currentColorIndex = _colorConfig.Count - 1;
			SetColorText(_colorConfig[_currentColorIndex]);

        }
        else if (_currentColorIndex + direction > _colorConfig.Count - 1)
        {
            _currentColorIndex = 0;
			SetColorText(_colorConfig[_currentColorIndex]);
		}
        else
        {
            _currentColorIndex = _currentColorIndex + direction;
			SetColorText(_colorConfig[_currentColorIndex]);
		}

        _takenPopup.gameObject.SetActive(false);
        PhotonNetwork.LocalPlayer.SetColorProperty(_colorConfig[_currentColorIndex]);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.HasColorProperty() || targetPlayer != ThisPlayer)
            return;
        if (changedProps.TryGetColorProperty(out string color))
		{
			SetColorText(color);
			photonView.RPC(REQUEST_COLOR, RpcTarget.MasterClient);
        }
        else
        {
            Debug.LogWarning($"Failed to get new player color. Sender: {targetPlayer.NickName}");
        }
    }

    [PunRPC]
    public void RequestColor(PhotonMessageInfo info)
    {
		if (!info.Sender.TryGetColorProperty(out string senderColor))
		{
            Debug.LogWarning($"{info.Sender} requested with empry color property.");
			return;
		}

		int colorCopies = 0;
		foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.TryGetColorProperty(out var color) && color == senderColor)
				colorCopies++;
        }
        photonView.RPC(SET_TAKEN_POPUP, info.Sender, colorCopies > 1);
    }

    [PunRPC]
    public void SetTakenPopup(bool state)
    {
        _takenPopup.gameObject.SetActive(state);
    }
}
