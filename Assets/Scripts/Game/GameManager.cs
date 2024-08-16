using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


namespace Game
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        private const int MENU_SCENE_INDEX = 0;
        private const float INACTIVITY_TTL = 3f;

        public static GameManager Instance;

        private List<PlayerCharacter> _activePlayers;
        private int _nextSpawnIndex = 0;

        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField] private GameObject _powerUpPrefab;
        [SerializeField] private UIManager _uiManager;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _activePlayers = new List<PlayerCharacter>();
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(SpawnPowerUpRoutine());
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PlayerCharacter.PlayerDied += OnPlayerDeath;
            PlayerCharacter.PlayerJoined += OnPlayerJoined;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PlayerCharacter.PlayerDied -= OnPlayerDeath;
            PlayerCharacter.PlayerJoined -= OnPlayerJoined;
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"New MasterClient is {newMasterClient.NickName}");
            _uiManager.UpdateText($"New MasterClient: {newMasterClient.NickName}");
            photonView.RPC(nameof(UpdateMasterClientUI), RpcTarget.All, newMasterClient.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RequestGameStateSync), newMasterClient);
                StartCoroutine(SpawnPowerUpRoutine());
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"{newPlayer.NickName} joined the room.");
            _uiManager.UpdateText($"{newPlayer.NickName} joined the room.");
            photonView.RPC(nameof(SyncGameState), newPlayer, _nextSpawnIndex);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Debug.Log($"{otherPlayer.NickName} left the room with inactive: {otherPlayer.IsInactive}");

            if (otherPlayer.CustomProperties.ContainsKey("IsInactive") && (bool)otherPlayer.CustomProperties["IsInactive"])
            {
                StartCoroutine(RemovePlayerAfterTTL(otherPlayer, INACTIVITY_TTL));
            }
        }

        private void OnPlayerDeath(PlayerCharacter player)
        {
            Debug.Log($"{player} eliminated");
            if (!PhotonNetwork.IsMasterClient)
                return;

            _activePlayers.Remove(player);
            Debug.Log($"{_activePlayers.Count} remaining.");
            if (_activePlayers.Count <= 1)
                TriggerGameOver(_activePlayers.Single().ThisPlayer.ActorNumber);
        }

        private void OnPlayerJoined(PlayerCharacter player) => _activePlayers.Add(player);

        private void TriggerGameOver(int winningPlayerActorNumber)
        {
            photonView.RPC(nameof(GameOver), RpcTarget.All, winningPlayerActorNumber);
        }

        private void SpawnPowerUp()
        {
            Transform spawnPoint = _spawnPoints[_nextSpawnIndex];
            PhotonNetwork.InstantiateRoomObject(_powerUpPrefab.name, spawnPoint.position, spawnPoint.rotation);
            _nextSpawnIndex = (_nextSpawnIndex + 1) % _spawnPoints.Count;
            photonView.RPC(nameof(UpdateNextSpawnIndex), RpcTarget.OthersBuffered, _nextSpawnIndex);
        }

        /*public override void OnLeftRoom()
        {
            SceneManager.LoadScene(MENU_SCENE_INDEX);
        }*/

        [PunRPC]
        private void GameOver(int winningPlayerActorNumber, PhotonMessageInfo info)
        {
            Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
            _uiManager.UpdateText(PhotonNetwork.LocalPlayer.ActorNumber == winningPlayerActorNumber
                ? "Game Over! You won."
                : "Game Over! You lost.");

            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(LeaveMatch());
        }

        [PunRPC]
        public void UpdateNextSpawnIndex(int index)
        {
            _nextSpawnIndex = index;
        }

        [PunRPC]
        public void SyncGameState(int nextSpawnIndex)
        {
            _nextSpawnIndex = nextSpawnIndex;
        }

        [PunRPC]
        private void RequestGameStateSync(PhotonMessageInfo info)
        {
            photonView.RPC(nameof(SyncGameState), info.Sender, _nextSpawnIndex);
        }

        [PunRPC]
        private void UpdateMasterClientUI(string newMasterClientName)
        {
            _uiManager.UpdateText($"New MasterClient: {newMasterClientName}");
        }

        [PunRPC]
        private void SetPlayerInactive(int actorNumber)
        {
            var player = PhotonNetwork.CurrentRoom.Players.Values.FirstOrDefault(p => p.ActorNumber == actorNumber);
            if (player != null)
            {
                // Set a custom property to indicate inactivity
                var properties = new ExitGames.Client.Photon.Hashtable { { "IsInactive", true } };
                player.SetCustomProperties(properties);
            }
        }

        private IEnumerator LeaveMatch()
        {
            yield return new WaitForSeconds(2f);
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
        }

        private IEnumerator SpawnPowerUpRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);
                SpawnPowerUp();
            }
        }
        private IEnumerator RemovePlayerAfterTTL(Player player, float ttl)
        {
            yield return new WaitForSeconds(ttl);

            if (PhotonNetwork.CurrentRoom.Players.ContainsKey(player.ActorNumber))
            {
                // Remove player from active players list
                var playerCharacter = _activePlayers.FirstOrDefault(p => p.ThisPlayer.ActorNumber == player.ActorNumber);
                if (playerCharacter != null)
                {
                    _activePlayers.Remove(playerCharacter);
                    PhotonNetwork.Destroy(playerCharacter.gameObject); // Destroy player object if necessary
                }

                // Optionally, handle the player leaving the room
                PhotonNetwork.CloseConnection(player);
            }
        }
    }
}
