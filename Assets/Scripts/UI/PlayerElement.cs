using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerElement : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _playerName;

    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _leftArrow;
    [SerializeField] private TextMeshProUGUI _colorTextbox;
    [SerializeField] private string[] _colors;
    private Player _thisPlayer;
    private int _currentColorIndex = 0;

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
            }
            else
            {
                Debug.Log($"Failed to get new player color. Sender: {targetPlayer.NickName}");
            }
        }
    }
}
