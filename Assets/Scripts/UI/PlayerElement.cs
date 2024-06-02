using TMPro;
using UnityEngine;
using Photon.Realtime;

public class PlayerElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerName;

	public void SetProperties(Player player)
	{
		SetTexts(player.NickName);
	}

	private void SetTexts(string playerName)
	{
		_playerName.text = playerName;
	}
}
