using UnityEngine;

namespace Game.Player.Visuals
{
    public class TileColor : MonoBehaviour
    {
        // get pawn from cursor
        [SerializeField] private Cursor _cursor;
        private void OnEnable()
        {
            _cursor.StateChanged += OnChangeState;
        }

        private void OnDisable()
        {
            _cursor.StateChanged -= OnChangeState;
        }

        private void OnChangeState(Cursor.State state)
        {
            Debug.Log("Did I Work");
        }
    }
}