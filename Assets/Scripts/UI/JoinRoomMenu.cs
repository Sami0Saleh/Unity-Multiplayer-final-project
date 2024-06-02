using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using Photon.Pun;
using Photon.Realtime;

public class JoinRoomMenu : MonoBehaviourPunCallbacks
{
	[SerializeField] private RoomElement _roomElementPrefab;
	[SerializeField] private Button _createRoomButton;
	[SerializeField] private Button _joinRandomRoomButton;
	[SerializeField] private Button _backButton;
	[SerializeField] private Transform _roomList;

	[SerializeField] private Toggle _hideFullRooms;

	private void Awake()
	{
		InitPool(_roomElementPrefab);
		_dict = new(ELEMENT_LIST_CAPACITY);
	}

	private void Start()
	{
		_createRoomButton.onClick.AddListener(CreateRoomButton);
		_joinRandomRoomButton.onClick.AddListener(JoinRandomRoomButton);
		_backButton.onClick.AddListener(BackButton);
		_hideFullRooms.onValueChanged.AddListener(ApplySearchFiltersOnToggle);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		ClearAllRooms();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (RoomInfo roomInfo in roomList)
		{
			if (roomInfo.RemovedFromList)
				UpdateClosedRoom(roomInfo);
			else
				UpdateOpenRoom(roomInfo);
		}
		ApplySearchFilters();
	}

	public void CreateRoomButton()
	{
		MainMenuManager.Instance.ToCreateRoomMenu();
	}

	public void JoinRandomRoomButton()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public void BackButton()
	{
		PhotonNetwork.LeaveLobby();
	}

	private void ApplySearchFiltersOnToggle(bool _)
	{
		ApplySearchFilters();
	}

	private void ApplySearchFilters()
	{
		foreach(var room in _dict.Values)
			room.gameObject.SetActive(ApplySearchFilters(room.RoomInfo));
	}

	private bool ApplySearchFilters(RoomInfo source)
	{
		if (_hideFullRooms.isOn && source.PlayerCount >= source.MaxPlayers)
			return false;
		return true;
	}

	#region ROOM_ELEMENT
	private const int ELEMENT_LIST_CAPACITY = 10;

	private Dictionary<string, RoomElement> _dict;

	private void UpdateOpenRoom(RoomInfo roomInfo)
	{
		if (!_dict.TryGetValue(roomInfo.Name, out RoomElement roomElement))
			roomElement = _pool.Get();
		roomElement.SetProperties(roomInfo);
		_dict.TryAdd(roomInfo.Name, roomElement);
	}

	private bool UpdateClosedRoom(RoomInfo roomInfo) => UpdateClosedRoom(roomInfo.Name);

	private bool UpdateClosedRoom(string roomName)
	{
		if (_dict.TryGetValue(roomName, out RoomElement roomElement))
		{
			_pool.Release(roomElement);
			_dict.Remove(roomName);
			return true;
		}
		return false;
	}

	private void ClearAllRooms()
	{
		foreach (var room in _dict)
			_pool.Release(room.Value);
		_dict.Clear();
	}
	#endregion

	#region POOL
	private ObjectPool<RoomElement> _pool;

	private void InitPool(RoomElement roomElement)
	{
		_pool = new
		(
			createFunc: () => CreateElement(roomElement),
			actionOnGet: OnGetElement,
			actionOnRelease: OnRelease,
			actionOnDestroy: DestroyElement,
			defaultCapacity: ELEMENT_LIST_CAPACITY
		);
	}

	private RoomElement CreateElement(RoomElement roomElement) => Instantiate(roomElement, _roomList);

	private void OnGetElement(RoomElement roomElement) => roomElement.gameObject.SetActive(true);

	private void DestroyElement(RoomElement roomElement) => Destroy(roomElement.gameObject);

	private void OnRelease(RoomElement roomElement) => roomElement.gameObject.SetActive(false);

	private bool ElementActive(RoomElement roomElement) => roomElement.gameObject.activeSelf;
	#endregion
}
