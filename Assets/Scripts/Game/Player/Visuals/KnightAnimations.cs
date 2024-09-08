using UnityEngine;
using DG.Tweening;
using static Game.Board.BoardMask;

namespace Game.Player.Visuals
{
	public class KnightAnimations : MonoBehaviour
	{
		private const string POINT_TRIGGER = "TrPoint";
        private const string WALK_BOOL = "isWalking";
        private const string FALL_TRIGGER = "TrFall";

        private const float TIME_TO_MOVE = 0.5f;
		private const float TIME_TO_DESTROY = 3f;
        private const float DISTANCE_TO_FALL = -10f;

		[SerializeField] private Animator _animator;
		[SerializeField] private Pawn _pawn;
		
		private Hammer Hammer => _pawn.Hammer;
		private PawnMovement Movement => _pawn.Movement;

        private void OnEnable()
        {
			Pawn.PlayerEliminated += Fall;
			Hammer.OnHammered += PointAt;
			Movement.OnPawnMoved += Walk;
        }

        private void OnDisable()
        {
            Pawn.PlayerEliminated -= Fall;
            Hammer.OnHammered -= PointAt;
            Movement.OnPawnMoved -= Walk;
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
		public void Walk(PawnMovement.PawnMovementEvent movement)
		{
			if (_animator != null)
			{
                _animator.SetBool(WALK_BOOL, true);
				transform.position = Board.Instance.Grid.CellToWorld(Board.PositionToCell(movement.steps[0]));
				Vector3[] path = new Vector3[movement.steps.Length];
				for (int i = 0; i < movement.steps.Length; i++)
					path[i] = Board.Instance.Grid.CellToWorld(Board.PositionToCell(movement.steps[i]));
				transform.DOPath(path, TIME_TO_MOVE * movement.steps.Length, PathType.Linear).SetLookAt(0.1f).SetEase(Ease.Linear).OnComplete(() => StopWalk(movement.Pawn));
            }
		}

		[ContextMenu("StopWalk")]
		public void StopWalk(Pawn pawn)
		{
			if (_animator != null && pawn == _pawn)
                _animator.SetBool(WALK_BOOL, false);
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