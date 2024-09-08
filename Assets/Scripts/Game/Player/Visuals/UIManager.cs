using System.Collections;
using TMPro;
using UnityEngine;
using PunPlayer = Photon.Realtime.Player;


namespace Game.Player.Visuals
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        private const string GAME_OVER_TEXT = "Game Over\n";

        [SerializeField] private TMP_Text _turnText;
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
            _ranksText.text = GAME_OVER_TEXT;
        }
        private void OnEnable()
        {
            TurnIterator.Instance.OnTurnChange += ChangeTurn;
            GameManager.Instance.GameOver += OnGameOver;
        }

        private void OnDisable()
        {
            TurnIterator.Instance.OnTurnChange -= ChangeTurn;
            GameManager.Instance.GameOver -= OnGameOver;
        }
        public void ChangeTurn(TurnIterator.TurnChangeEvent turn)
        {
            _turnText.text = $"{turn.currentPlayer.NickName}'s turn";
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
