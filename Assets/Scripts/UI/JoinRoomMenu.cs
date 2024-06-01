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

	private void Awake()
	{
		InitPool(_roomElementPrefab);
	}

	private void Start()
	{
		_createRoomButton.onClick.AddListener(CreateRoomButton);
		_joinRandomRoomButton.onClick.AddListener(JoinRandomRoomButton);
		_backButton.onClick.AddListener(BackButton);
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		HideAllElements();
		foreach (RoomInfo roomInfo in roomList)
			_pool.Get().SetProperties(roomInfo);
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

	#region POOL
	private ObjectPool<RoomElement> _pool;
	private List<RoomElement> _active;
	private const int DEFAULT_POOL_CAPACITY = 10;

	private void InitPool(RoomElement roomElement)
	{
		_pool = new ObjectPool<RoomElement>
		(
			createFunc: () => CreateElement(roomElement),
			actionOnGet: OnGetElement,
			actionOnRelease: OnRelease,
			actionOnDestroy: DestroyElement,
			defaultCapacity: DEFAULT_POOL_CAPACITY
		);
	}

	private void HideAllElements()
	{
		foreach (var element in _active)
			_pool.Release(element);
	}

	private RoomElement CreateElement(RoomElement roomElement) => Instantiate(roomElement, _roomList);

	private void OnGetElement(RoomElement roomElement)
	{
		_active.Add(roomElement);
		roomElement.gameObject.SetActive(true);
	}

	private void DestroyElement(RoomElement roomElement) => Destroy(roomElement.gameObject);

	private void OnRelease(RoomElement roomElement)
	{
		_active.Remove(roomElement);
		roomElement.gameObject.SetActive(false);
	}

	private bool ElementActive(RoomElement roomElement) => roomElement.gameObject.activeSelf;
	#endregion
}
