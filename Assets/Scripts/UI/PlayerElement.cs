using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Game.Player.Visuals;

namespace UI
{
	public class PlayerElement : MonoBehaviourPunCallbacks
	{
		const string REQUEST_COLOR = nameof(RequestColor);
		const string RETURN_COLOR = nameof(ReturnColor);

		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private TextMeshProUGUI _playerName;
		[SerializeField] private Button _rightArrow;
		[SerializeField] private Button _leftArrow;
		[SerializeField] private TextMeshProUGUI _colorTextbox;
		[SerializeField] private TextMeshProUGUI _takenPopup;
		private int _currentColorIndex = -1;

		public Player ThisPlayer => photonView.Owner;

		private void Start()
		{
			AddToPlayerGroup();
			if (ThisPlayer.TryGetColorProperty(out var color))
				_currentColorIndex = _colorConfig.IndexOf(color);
			SetNameText(ThisPlayer.NickName);
			SetColorText(_colorConfig[_currentColorIndex]);

			if (!photonView.IsMine)
				return;

			BindButtons();

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

		private void SetColorText(string color)
		{
			_colorTextbox.text = color;
			if (_colorConfig.TryGetValue(color, out var mat))
				_colorTextbox.color = mat.color;
		}

		private void CycleColorRightButton() => CycleColor(1);

		private void CycleColorLeftButton() => CycleColor(-1);

		private void CycleColor(int direction)
		{
			Debug.Assert(photonView.IsMine, "Cannot cycle colors on another player's PlayerElement.");
			ModifyCurrentColorIndex();
			SetColorText(_colorConfig[_currentColorIndex]);
			_takenPopup.gameObject.SetActive(false);
			StopAllCoroutines();
			StartCoroutine(RequestColorDelay());
			PhotonNetwork.LocalPlayer.SetColorProperty(_colorConfig[_currentColorIndex]);

			void ModifyCurrentColorIndex() => _currentColorIndex = Utility.Modulo(_currentColorIndex + direction, _colorConfig.Count);

			IEnumerator RequestColorDelay(float delay = 1f)
			{
				yield return new WaitForSeconds(delay);
				photonView.RPC(REQUEST_COLOR, RpcTarget.MasterClient);
			}
		}

		public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
		{
			if (!changedProps.HasColorProperty() || targetPlayer != ThisPlayer)
				return;
			if (changedProps.TryGetColorProperty(out string color))
			{
				SetColorText(color);
			}
			else
			{
				Debug.LogWarning($"Failed to get new player color. Sender: {targetPlayer.NickName}");
			}
		}

		[PunRPC]
		public void RequestColor(PhotonMessageInfo info)
		{
			Debug.Assert(PhotonNetwork.IsMasterClient, "Only the master client may be issued color requests.");
			if (!info.Sender.TryGetColorProperty(out string senderColor))
			{
				Debug.LogWarning($"{info.Sender} requested with empty color property.");
				return;
			}

			int colorCopies = 0;
			foreach (var player in PhotonNetwork.PlayerList)
			{
				if (player.TryGetColorProperty(out var color) && color == senderColor)
					colorCopies++;
			}
			photonView.RPC(RETURN_COLOR, info.Sender, colorCopies > 1);
		}

		[PunRPC]
		public void ReturnColor(bool state)
		{
			_takenPopup.gameObject.SetActive(state);
		}
	}
}