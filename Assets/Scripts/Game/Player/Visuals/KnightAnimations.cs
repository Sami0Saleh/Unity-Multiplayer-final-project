using UnityEngine;

namespace Game.Player.Visuals
{
	public class KnightAnimations : MonoBehaviour
	{
		const string POINT_TRIGGER = "TrPoint";
		const string WALK_TRIGGER = "TrWalk";
		const string STOP_WALK_TRIGGER = "TrStopWalk";

		[SerializeField] private Animator _animator;

		/// <summary>
		/// Rotate character to desired tile.
		/// </summary>
		[ContextMenu("PointAt")]
		public void PointAt()
		{
			if (_animator != null)
				_animator.SetTrigger(POINT_TRIGGER);
		}

		[ContextMenu("Walk")]
		public void Walk()
		{
			//Rotate character to desired tile
			if (_animator != null)
				_animator.SetTrigger(WALK_TRIGGER);
		}

		[ContextMenu("StopWalk")]
		public void StopWalk()
		{
			//Rotate character to desired tile
			if (_animator != null)
				_animator.SetTrigger(STOP_WALK_TRIGGER);
		}
	}
}