using UnityEngine;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace Game.Player
{
	public class Cursor : MonoBehaviourPun
	{
		[SerializeField] private MousePositionTracker _mousePositionTracker;

		public PunPlayer Owner => photonView.Owner;
		public Pawn OwnerPawn => GameManager.Instance.ActivePlayers[Owner];

		private void Awake()
		{
			OwnerPawn.Cursor = this;
			gameObject.name = $"{Owner.NickName}'s Cursor";
			if (!photonView.AmOwner)
				Destroy(_mousePositionTracker);
		}
	}
}
