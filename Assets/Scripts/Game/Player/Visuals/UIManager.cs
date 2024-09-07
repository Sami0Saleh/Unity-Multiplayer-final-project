using TMPro;
using UnityEngine;


namespace Game.Player.Visuals
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Cursor _cursor;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _actionText;

        private void OnEnable()
        {
            _cursor.StateChanged += OnStateChange;
            TurnIterator.Instance.OnTurnChange += ChangeTurn;
        }

        private void OnDisable()
        {
            _cursor.StateChanged -= OnStateChange;
            TurnIterator.Instance.OnTurnChange -= ChangeTurn;
        }
        public void ChangeTurn(TurnIterator.TurnChangeEvent turn)
        {
            _turnText.text = $"{turn.currentPlayer.NickName}'s turn";
        }

        private void OnStateChange(Cursor.State state)
        {
            
            if (state == Cursor.State.Move)
            {
                Debug.Log("Move");
                _actionText.text = "Move your pawn.";
            }

            else if (state == Cursor.State.Hammer)
            {
                Debug.Log("Hammer");

                _actionText.text = "Choose a tile to destroy.";
            }
            
        }
    }
}
