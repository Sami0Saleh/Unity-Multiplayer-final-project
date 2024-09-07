using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Game.Board.BoardMask;

namespace Game.Player.Visuals
{
	public class KnightAnimations : MonoBehaviour
	{
		const string POINT_TRIGGER = "TrPoint";
		const string WALK_TRIGGER = "TrWalk";
		const string STOP_WALK_TRIGGER = "TrStopWalk";
		const string FALL_TRIGGER = "TrFall";

		const float TIME_TO_DESTROY = 3f;
		const float DISTANCE_TO_FALL = -10f;

		[SerializeField] private Animator _animator;
		[SerializeField] private Pawn _pawn;
		
		private Hammer Hammer => _pawn.Hammer;

        private void OnEnable()
        {
			Pawn.PlayerEliminated += Fall;
			Hammer.OnHammered += PointAt;
        }

        private void OnDisable()
        {
            Hammer.OnHammered -= PointAt;
        }

        /// <summary>
        /// Rotate character to desired tile.
        /// </summary>
        [ContextMenu("PointAt")]
		public void PointAt(Position pos)
		{
            if (_animator != null)
			{
                transform.LookAt(Board.Instance.Grid.CellToWorld(Board.PositionToCell(pos)));
                _animator.SetTrigger(POINT_TRIGGER);
            }
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

		[ContextMenu("Fall")]
		public void Fall(Pawn pawn)
		{
            if (_animator != null && pawn == _pawn)
			{
                _animator.SetTrigger(FALL_TRIGGER);
                transform.SetParent(null);
                transform.DOMoveY(DISTANCE_TO_FALL, TIME_TO_DESTROY).SetEase(Ease.InExpo).OnComplete(() => Destroy(gameObject));
            }
        }
	}
}