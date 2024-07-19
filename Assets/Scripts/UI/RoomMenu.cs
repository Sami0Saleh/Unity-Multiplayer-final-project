using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class RoomMenu : MonoBehaviourPunCallbacks
{
	const int GAME_SCENE_INDEX = 1;
	const string DISPALY_CHAT = nameof(DisplayChatMessage);
	const string PLAYER_ELEMENT_PREFAB = "Player Element Photon";

	[SerializeField] private PlayerColors _colorConfig;
	[SerializeField] private PlayerElement _playerElementPrefab;
	[SerializeField] private ChatMessage _chatMessagePrefab;
	[SerializeField] private PhotonView _photonView;
	[SerializeField] private Button _leaveRoomButton;
	[SerializeField] private Button _startButton;
	[SerializeField] private TextMeshProUGUI _playerCountText;
	[SerializeField] private Transform _chat;
	[SerializeField] private TMP_InputField _chatBoxInput;

	private bool StartCondition => PhotonNetwork.CurrentRoom.PlayerCount > 1 && AllUniqueAndValidColors();

	[field: SerializeField] public Transform PlayerList { get; private set; }

	private void Start()
	{
		_leaveRoomButton.onClick.AddListener(LeaveRoomButton);
		_startButton.onClick.AddListener(StartButton);
		_chatBoxInput.onEndEdit.AddListener(SendChatButton);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		CreateElement();
		UpdatePlayerCount();
		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.CurrentRoom.IsOpen = true;
	}

    public override void OnDisable()
	{
		base.OnDisable();
		ClearChat();
	}

	public void LeaveRoomButton()
	{
		PhotonNetwork.LeaveRoom();
    }

	public void StartButton()
	{
		if (StartCondition)
		{
			PhotonNetwork.DestroyAll();
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.LoadLevel(GAME_SCENE_INDEX);
		}
	}

	public void SendChatButton(string msg)
	{
		if (msg.IsNullOrEmpty())
			return;
		_photonView.RPC(DISPALY_CHAT, RpcTarget.All, msg);
		_chatBoxInput.text = string.Empty;
	}

	[PunRPC]
	public void DisplayChatMessage(string message, PhotonMessageInfo info)
	{
		var msg = Instantiate(_chatMessagePrefab, _chat);
		msg.Text = $"{info.Sender.NickName}: {message}";
		if (_colorConfig.TryGetMaterial(info.Sender, out var mat))
			msg.Color = mat.color;
	}

	private bool AllUniqueAndValidColors()
	{
		var colors = GetAllColorsInLobby();
		return colors.Distinct().Count() == PhotonNetwork.CurrentRoom.PlayerCount &&
			!colors.Contains(PlayerColors.DEFAULT_COLOR);
	}

	private void UpdatePlayerCount()
	{
        UpdateStartButton();
        UpdatePlayerCountText();
	}

	private void UpdateStartButton()
	{
		_startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
		if (PhotonNetwork.IsMasterClient)
			_startButton.interactable = StartCondition;
	}

	private IEnumerable<string> GetAllColorsInLobby()
	{
		foreach (var player in PhotonNetwork.PlayerList)
		{
			if (player.CustomProperties.TryGetValue("PlayerColor", out var color))
				yield return color.ToString();
		}
	}

	private void UpdatePlayerCountText()
	{
		var room = PhotonNetwork.CurrentRoom;
		_playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
	}

	public override void OnPlayerEnteredRoom(Player newPlayer) => UpdatePlayerCount();

	public override void OnPlayerLeftRoom(Player otherPlayer) => UpdatePlayerCount();

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
		UpdateStartButton();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if (!PhotonNetwork.IsMasterClient)
			return;
		UpdatePlayerCount();
		UpdateStartButton();
	}

	private void ClearChat()
	{
		for (int i = _chat.childCount-1; i >= 0; i--)
			Destroy(_chat.GetChild(i).gameObject);
	}

	private PlayerElement CreateElement() => PhotonNetwork.Instantiate(PLAYER_ELEMENT_PREFAB, PlayerList.position, PlayerList.rotation).GetComponent<PlayerElement>();
}
