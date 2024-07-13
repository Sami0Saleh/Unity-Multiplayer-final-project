using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using WebSocketSharp;
using static UnityEditor.Progress;
using System.Linq;

public class RoomMenu : MonoBehaviourPunCallbacks
{
	const int GAME_SCENE_INDEX = 1;
	const string DISPALY_CHAT = nameof(DisplayChatMessage);
	[SerializeField] private PlayerElement _playerElementPrefab;
	[SerializeField] private ChatMessage _chatMessagePrefab;
	[SerializeField] private PhotonView _photonView;
	[SerializeField] private Button _leaveRoomButton;
	[SerializeField] private Button _startButton;
	[SerializeField] private TextMeshProUGUI _playerCountText;
	[SerializeField] private Transform _playerList;
	[SerializeField] private Transform _chat;
	[SerializeField] private TMP_InputField _chatBoxInput;

	private bool StartCondition => PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;

	private void Awake()
	{
		InitPool(_playerElementPrefab);
		_dict = new(ELEMENT_LIST_CAPACITY);
	}

	private void Start()
	{
		_leaveRoomButton.onClick.AddListener(LeaveRoomButton);
		_startButton.onClick.AddListener(StartButton);
		_chatBoxInput.onEndEdit.AddListener(SendChatButton);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		foreach (var player in PhotonNetwork.PlayerList)
			OnPlayerEnteredRoom(player);
	}

    public override void OnDisable()
	{
		base.OnDisable();
		ClearChat();
		ClearAllPlayers();
	}

	public void LeaveRoomButton()
	{
		PhotonNetwork.LeaveRoom();
    }

	public void StartButton()
	{
		if (StartCondition)
			PhotonNetwork.LoadLevel(GAME_SCENE_INDEX);
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
	}

	private void UpdatePlayerCount()
	{
		StartCoroutine(CheckPlayerUniqueColor());
		UpdatePlayerCountText();
	}

    private IEnumerator CheckPlayerUniqueColor()
    {
		yield return new WaitForSeconds(0.1f);
		List<string> playerColors = new List<string>();
		foreach (var player in PhotonNetwork.PlayerList)
		{
			playerColors.Add(player.CustomProperties["PlayerColor"].ToString());
			string allPlayerStrings = "";
			foreach (var item in playerColors)
			{
				allPlayerStrings = allPlayerStrings + item + ", ";
            }
            Debug.Log(allPlayerStrings);
        }
		if (playerColors.Distinct().Count() == playerColors.Count())
		{
            _startButton.interactable = StartCondition;
        }
		else
		{
			_startButton.interactable = false;
		}
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		PlayerElement playerElement = _pool.Get();
		if (_dict.TryAdd(newPlayer.ActorNumber, playerElement))
			playerElement.SetProperties(newPlayer);
		else
			_pool.Release(playerElement);
		UpdatePlayerCount();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (_dict.TryGetValue(otherPlayer.ActorNumber, out PlayerElement playerElement))
		{
			_pool.Release(playerElement);
			_dict.Remove(otherPlayer.ActorNumber);
		}
		UpdatePlayerCount();
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
		if (_dict.TryGetValue(targetPlayer.ActorNumber, out PlayerElement playerElement))
			playerElement.SetProperties(targetPlayer);
		StartCoroutine(CheckPlayerUniqueColor());
	}

	private void UpdatePlayerCountText()
	{
		var room = PhotonNetwork.CurrentRoom;
		_playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
	}

	private void ClearChat()
	{
		for (int i = 0; i < _chat.childCount; i++)
			Destroy(_chat.GetChild(i).gameObject);
	}

	#region ROOM_ELEMENT
	private const int ELEMENT_LIST_CAPACITY = 10;

	private Dictionary<int, PlayerElement> _dict;

	private void ClearAllPlayers()
	{
		foreach (var room in _dict)
			_pool.Release(room.Value);
		_dict.Clear();
	}
	#endregion

	#region POOL
	private ObjectPool<PlayerElement> _pool;

	private void InitPool(PlayerElement playerElement)
	{
		_pool = new
		(
			createFunc: () => CreateElement(playerElement),
			actionOnGet: OnGetElement,
			actionOnRelease: OnRelease,
			actionOnDestroy: DestroyElement,
			defaultCapacity: ELEMENT_LIST_CAPACITY
		);
	}

	private PlayerElement CreateElement(PlayerElement playerElement) => Instantiate(playerElement, _playerList);

	private void OnGetElement(PlayerElement playerElement) => playerElement.gameObject.SetActive(true);

	private void DestroyElement(PlayerElement playerElement) => Destroy(playerElement.gameObject);

	private void OnRelease(PlayerElement playerElement) => playerElement.gameObject.SetActive(false);

	private bool ElementActive(PlayerElement playerElement) => playerElement.gameObject.activeSelf;
	#endregion
}
