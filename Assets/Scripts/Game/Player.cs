using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class Player : MonoBehaviour
	{
		[SerializeField] private GameObject _cursorPrefab;
		[SerializeField, HideInInspector] private PhotonView _photonView;

		private void OnValidate()
		{
			_photonView = GetComponent<PhotonView>();
		}

		private void Awake()
		{
			if (!_photonView.IsMine)
			{
				enabled = false;
				return;
			}
			PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);
		}
	}
}
