using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class PlayerElement : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _playerName;

    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _leftArrow;
    [SerializeField] private TextMeshProUGUI _colorTextbox;
    [SerializeField] private string[] _colors;
    private RoomMenu _roomMenu;
    private Player _thisPlayer;
    private int _currentColorIndex = 0;

    const string IS_COLOR_UNIQUE = nameof(IsColorUnique);
    const string SET_TAKEN_POPUP = nameof(SetTakenPopup);
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private TextMeshProUGUI _takenPopup;

    public void SetProperties(Player player)
    {
        SetTexts(player.NickName);
        _thisPlayer = player;
    }

    private void SetTexts(string playerName)
    {
        _playerName.text = playerName;
    }

    private void Start()
    {
        _roomMenu = FindFirstObjectByType<RoomMenu>();
        transform.parent = _roomMenu.PlayerList;
        transform.localScale = Vector3.one;
        _rightArrow.onClick.AddListener(CycleColorRight);
        _leftArrow.onClick.AddListener(CycleColorLeft);

        _colorTextbox.text = _colors[_currentColorIndex];
        if (PhotonNetwork.LocalPlayer.UserId == _thisPlayer.UserId)
        {
            _rightArrow.gameObject.SetActive(true);
            _leftArrow.gameObject.SetActive(true);
        }

        var _hashedProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        if (!_hashedProperties.ContainsKey("PlayerColor"))
        {
            _hashedProperties.Add("PlayerColor", _colors[_currentColorIndex]);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(_hashedProperties);
        Debug.Log(PhotonNetwork.LocalPlayer.ToStringFull());
    }

    private void OnDisable()
    {
        if (PhotonNetwork.LocalPlayer.UserId == _thisPlayer.UserId)
        {
            _rightArrow.gameObject.SetActive(false);
            _leftArrow.gameObject.SetActive(false);
            _takenPopup.gameObject.SetActive(false);
        }
    }

    private void CycleColorRight()
    {
        CycleColor(1);
        Debug.Log($"Cycled right. New Color is {_colors[_currentColorIndex]}, indexed {_currentColorIndex}");
    }

    private void CycleColorLeft()
    {
        CycleColor(-1);
        Debug.Log($"Cycled left. New Color is {_colors[_currentColorIndex]}, indexed {_currentColorIndex}");
    }

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
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (!changedProps.ContainsKey("PlayerColor"))
            return;
        if (targetPlayer.UserId == _thisPlayer.UserId)
        {
            if (changedProps.TryGetValue("PlayerColor", out object newText))
            {
                _colorTextbox.text = newText.ToString();
                _photonView.RPC(IS_COLOR_UNIQUE, RpcTarget.MasterClient, _thisPlayer);
            }
            else
            {
                Debug.Log($"Failed to get new player color. Sender: {targetPlayer.NickName}");
            }
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
