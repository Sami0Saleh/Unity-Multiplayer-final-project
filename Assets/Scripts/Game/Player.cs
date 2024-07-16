using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class Player : MonoBehaviourPun
	{
		[SerializeField] private GameObject _cursorPrefab;

		private void Awake()
		{
			if (!photonView.IsMine)
			{
				enabled = false;
				return;
			}
			PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);
		}
	}
}
