using UnityEngine;
using Photon.Pun;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;

		private void Awake()
		{
			var owner = photonView.Owner;
			GameManager.Instance.ActivePlayers[owner].Cursor = this;
			gameObject.name = $"{owner.NickName}'s Cursor";
			if (!photonView.AmOwner)
				Destroy(_mousePositionTracker);
		}
	}
}
