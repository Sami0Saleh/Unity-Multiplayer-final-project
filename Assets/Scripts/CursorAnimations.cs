using Game.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Game.Player.Visuals
{
    public class CursorAnimations : MonoBehaviour
    {
        [SerializeField] private Cursor _cursor;
        private PawnMovement Movement => _cursor.OwnerPawn.Movement;

        public UnityEvent OnMove;

        private void OnEnable()
        {
            Movement.OnPawnMoved += DoBootsAnimation;
        }
        private void OnDisable()
        {
            Movement.OnPawnMoved -= DoBootsAnimation;
        }

        private void DoBootsAnimation(PawnMovement.PawnMovementEvent movement)
        {
            OnMove?.Invoke();
        }
    }
}