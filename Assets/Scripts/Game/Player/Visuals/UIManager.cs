using TMPro;
using UnityEngine;
using PunPlayer = Photon.Realtime.Player;


namespace Game.Player.Visuals
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public Cursor Cursor;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _actionText;
        
        [SerializeField] private GameObject _ranks;
        [SerializeField] private TMP_Text _ranksText;

        private void Awake()
        {
            if (!TryRegisterSingleton())
                return;

            bool TryRegisterSingleton()
            {
                bool created = Instance == null;
                if (created)
                    Instance = this;
                else
                    Destroy(gameObject);
                return created;
            }
        }
        private void Start()
        {
            _ranksText.text = "Ranks:\n";
        }
        private void OnEnable()
        {
            Cursor.StateChanged += OnStateChange;
            TurnIterator.Instance.OnTurnChange += ChangeTurn;
            GameManager.Instance.GameOver += OnGameOver;
        }

        private void OnDisable()
        {
            Cursor.StateChanged -= OnStateChange;
            TurnIterator.Instance.OnTurnChange -= ChangeTurn;
            GameManager.Instance.GameOver -= OnGameOver;
        }
        public void ChangeTurn(TurnIterator.TurnChangeEvent turn)
        {
            _turnText.text = $"{turn.currentPlayer.NickName}'s turn";
        }

        private void OnStateChange(Cursor.State state)
        {
            if (state == Cursor.State.Neutral)
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
        
        private void OnGameOver(PunPlayer player)
        {
            _ranks.SetActive(true);
        }
        public void UpdateRanks(string text)
        {
            _ranksText.text += text;
        }
    }
}
