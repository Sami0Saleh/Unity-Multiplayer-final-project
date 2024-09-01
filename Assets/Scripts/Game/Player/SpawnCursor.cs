using UnityEngine;
using Photon.Pun;

namespace Game.Player
{
	public class SpawnCursor : MonoBehaviourPun
	{
		[SerializeField] private Cursor _cursorPrefab;

		private void Start()
		{
			if (!photonView.AmOwner)
				return;
			var cursor = PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation).GetComponent<Cursor>();
			GameManager.Instance.ActivePlayers[photonView.Owner].Cursor = cursor;
			Destroy(this);
		}
	}
}
