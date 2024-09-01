using UnityEngine;
using Photon.Pun;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;

		private void Awake()
		{
			gameObject.name = $"{photonView.Owner.NickName}'s Cursor";
			if (!photonView.AmOwner)
				_mousePositionTracker.enabled = false;
		}
	}
}
