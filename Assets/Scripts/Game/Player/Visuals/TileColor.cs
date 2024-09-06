using UnityEngine;

namespace Game.Player.Visuals
{
    public class TileColor : MonoBehaviour
    {
        // get pawn from cursor
        [SerializeField] private Cursor _cursor;
        private Board _board;
        private void Start() => _board = Board.Instance;
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
            if (state == Cursor.State.Move)
            {
                foreach(var tile in _board.Tiles) 
                {
                    if (/*tiles are in range of the player*/ true)
                    tile.SetActive(false);
                }
            }
        }
    }
}