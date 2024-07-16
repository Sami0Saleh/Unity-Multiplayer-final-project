using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private const int GAME_SCENE_INDEX = 0;

    private const string GameOverRPC = "GameOver";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void GameOver(int losingPlayerActorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == losingPlayerActorNumber)
        {
            Debug.Log("Game Over! You lost.");
        }
        else
        {
            Debug.Log("Game Over! You won.");
        }

        PhotonNetwork.LeaveRoom();
        StartCoroutine(LoadRoomSelectionScene());
        
    }

    public void TriggerGameOver(int losingPlayerActorNumber)
    {
        photonView.RPC(GameOverRPC, RpcTarget.All, losingPlayerActorNumber);
    }

    IEnumerator LoadRoomSelectionScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(GAME_SCENE_INDEX);
        PhotonNetwork.Disconnect();
    }
}