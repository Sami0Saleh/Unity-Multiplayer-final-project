using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
		private const string GAME_OVER = nameof(GameOver);
		public static GameManager Instance;

		private List<PlayerCharacter> _activePlayers;

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

		[PunRPC]
		public void GameOver(int winningPlayerActorNumber, PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
			if (PhotonNetwork.LocalPlayer.ActorNumber == winningPlayerActorNumber)
				Debug.Log("Game Over! You won.");
			else
				Debug.Log("Game Over! You lost.");

			if (PhotonNetwork.IsMasterClient)
				StartCoroutine(LeaveMatch());
		}

		public void TriggerGameOver(int winningPlayerActorNumber)
		{
			photonView.RPC(GAME_OVER, RpcTarget.All, winningPlayerActorNumber);
		}

		private void OnPlayerDeath(PlayerCharacter player)
		{
			Debug.Log($"{player} eleminated");
			if (!PhotonNetwork.IsMasterClient)
				return;
			_activePlayers.Remove(player);
			Debug.Log($"{_activePlayers.Count} remaining.");
			if (_activePlayers.Count <= 1)
				TriggerGameOver(_activePlayers.Single().ThisPlayer.ActorNumber);
		}

		private void OnPlayerJoined(PlayerCharacter player) => _activePlayers.Add(player);

		IEnumerator LeaveMatch()
		{
			yield return new WaitForSeconds(2f);
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}