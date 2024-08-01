using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class PlayerCursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;

		private void Awake()
		{
			if (!photonView.AmOwner)
				_mousePositionTracker.enabled = false;
		}
	}
}
