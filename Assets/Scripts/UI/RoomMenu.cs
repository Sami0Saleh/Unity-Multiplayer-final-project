using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RoomMenu : MonoBehaviourPunCallbacks
{
	[SerializeField] private PlayerElement _playerElementPrefab;
	[SerializeField] private Button _leaveRoomButton;
	[SerializeField] private TextMeshProUGUI _playerCountText;
	[SerializeField] private Transform _playerList;

	private void Awake()
	{
		InitPool(_playerElementPrefab);
		_dict = new(ELEMENT_LIST_CAPACITY);
	}

	private void Start()
	{
		_leaveRoomButton.onClick.AddListener(LeaveRoomButton);
	}

	private void OnEnable()
	{
		foreach (var player in PhotonNetwork.PlayerList)
			OnPlayerEnteredRoom(player);
	}

	private void OnDisable()
	{
		ClearAllPlayers();
	}

	public void LeaveRoomButton()
	{
		PhotonNetwork.LeaveRoom();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		PlayerElement playerElement = _pool.Get();
		if (_dict.TryAdd(newPlayer.ActorNumber, playerElement))
			playerElement.SetProperties(newPlayer);
		else
			_pool.Release(playerElement);
		UpdatePlayerCountText();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (_dict.TryGetValue(otherPlayer.ActorNumber, out PlayerElement playerElement))
		{
			_pool.Release(playerElement);
			_dict.Remove(otherPlayer.ActorNumber);
		}
		UpdatePlayerCountText();
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (_dict.TryGetValue(targetPlayer.ActorNumber, out PlayerElement playerElement))
			playerElement.SetProperties(targetPlayer);
	}

	private void UpdatePlayerCountText()
	{
		var room = PhotonNetwork.CurrentRoom;
		_playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
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
