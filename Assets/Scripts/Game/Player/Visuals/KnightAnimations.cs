using UnityEngine;

namespace Game.Player.Visuals
{
	public class KnightAnimations : MonoBehaviour
	{
		const string POINT_TRIGGER = "TrPoint";

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
	}
}