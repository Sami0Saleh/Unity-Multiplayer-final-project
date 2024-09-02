using UnityEngine;
using Photon.Pun;

namespace Game.Player
{
	public class SpawnCursor : MonoBehaviourPun
	{
		[SerializeField] private Cursor _cursorPrefab;

		private void Start()
		{
			if (photonView.AmOwner)
				PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);
			Destroy(this);
		}
	}
}
