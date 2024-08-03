using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
		private const string GAME_OVER = nameof(OnGameOver);
		public static GameManager Instance;

		public event UnityAction<int> GameOver;

		public Dictionary<Player, PlayerCharacter> ActivePlayers { get; private set; }

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			ActivePlayers = new();
		}

		public override void OnEnable()
		{
			base.OnEnable();
			PlayerCharacter.PlayerEliminated += OnPlayerEliminated;
			PlayerCharacter.PlayerJoined += OnPlayerJoined;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			PlayerCharacter.PlayerEliminated -= OnPlayerEliminated;
			PlayerCharacter.PlayerJoined -= OnPlayerJoined;
		}

		[PunRPC]
		public void OnGameOver(int winningPlayerActorNumber, PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
			GameOver?.Invoke(winningPlayerActorNumber);

			if (PhotonNetwork.IsMasterClient)
				StartCoroutine(LeaveMatch());
		}

		public void TriggerGameOver(int winningPlayerActorNumber)
		{
			photonView.RPC(GAME_OVER, RpcTarget.All, winningPlayerActorNumber);
		}

		private void OnPlayerEliminated(PlayerCharacter player)
		{
			if (!PhotonNetwork.IsMasterClient)
				return;
			ActivePlayers.Remove(player.ThisPlayer);
			if (ActivePlayers.Count <= 1)
				TriggerGameOver(ActivePlayers.Single().Key.ActorNumber);
		}

		private void OnPlayerJoined(PlayerCharacter player) => ActivePlayers.Add(player.ThisPlayer, player);

		IEnumerator LeaveMatch()
		{
			yield return new WaitForSeconds(2f); // TODO Test without this delay
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}