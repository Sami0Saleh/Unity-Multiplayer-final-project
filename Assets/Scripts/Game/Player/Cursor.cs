using UnityEngine;
using Photon.Pun;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;

		private void Awake()
		{
			if (!photonView.AmOwner)
				_mousePositionTracker.enabled = false;
		}
	}
}
