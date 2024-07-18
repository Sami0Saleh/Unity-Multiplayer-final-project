using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerElement : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _leftArrow;
    [SerializeField] private TextMeshProUGUI _colorTextbox;
	[SerializeField] private TextMeshProUGUI _takenPopup;
	[SerializeField] private string[] _colors;
    private RoomMenu _roomMenu;
    private int _currentColorIndex = 0;

    const string IS_COLOR_UNIQUE = nameof(IsColorUnique);
    const string SET_TAKEN_POPUP = nameof(SetTakenPopup);

    public Player ThisPlayer => photonView.Owner;

	private void Start()
    {
        _roomMenu = MainMenuManager.Instance.RoomMenu;
        transform.SetParent(_roomMenu.PlayerList);
        transform.localScale = Vector3.one;
        _rightArrow.onClick.AddListener(CycleColorRightButton);
        _leftArrow.onClick.AddListener(CycleColorLeftButton);

        _colorTextbox.text = _colors[_currentColorIndex];
        _rightArrow.gameObject.SetActive(photonView.IsMine);
        _leftArrow.gameObject.SetActive(photonView.IsMine);

		SetNameText(ThisPlayer.NickName);
		var _hashedProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        if (!_hashedProperties.ContainsKey("PlayerColor"))
        {
            _hashedProperties.Add("PlayerColor", _colors[_currentColorIndex]);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(_hashedProperties);
        Debug.Log(PhotonNetwork.LocalPlayer.ToStringFull());
	}

	private void SetNameText(string playerName) => _playerName.text = playerName;

	private void CycleColorRightButton() => CycleColor(1);

	private void CycleColorLeftButton() => CycleColor(-1);

	private void CycleColor(int direction)
    {
        if (_currentColorIndex + direction < 0)
        {
            _currentColorIndex = _colors.Length - 1;
            _colorTextbox.text = _colors[_currentColorIndex];

        }
        else if (_currentColorIndex + direction > _colors.Length - 1)
        {
            _currentColorIndex = 0;
            _colorTextbox.text = _colors[_currentColorIndex];
        }
        else
        {
            _currentColorIndex = _currentColorIndex + direction;
            _colorTextbox.text = _colors[_currentColorIndex];
        }

        _takenPopup.gameObject.SetActive(false);

        var _hashedProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        _hashedProperties["PlayerColor"] = _colors[_currentColorIndex];
        PhotonNetwork.LocalPlayer.SetCustomProperties(_hashedProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("PlayerColor") || targetPlayer.UserId != ThisPlayer.UserId)
            return;
        if (changedProps.TryGetValue("PlayerColor", out object newText))
        {
            _colorTextbox.text = newText.ToString();
            photonView.RPC(IS_COLOR_UNIQUE, RpcTarget.MasterClient, ThisPlayer);
        }
        else
        {
            Debug.Log($"Failed to get new player color. Sender: {targetPlayer.NickName}");
        }
    }

    [PunRPC]
    public void IsColorUnique(Player sender)
    {
        int colorCopies = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("PlayerColor", out var color))
                if (color.ToString() == sender.CustomProperties.TryGetValue("PlayerColor", out var senderColor).ToString())
                    colorCopies++;
        }
        if (colorCopies > 1)
        {
            photonView.RPC(SET_TAKEN_POPUP, sender, true);
        }
        else
        {
            photonView.RPC(SET_TAKEN_POPUP, sender, false);
        }
    }

    [PunRPC]
    public void SetTakenPopup(bool state)
    {
        _takenPopup.gameObject.SetActive(state);
    }
}
