using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Game.Player;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
		public static GameManager Instance;

		public event UnityAction GameStart;
		public event UnityAction<Photon.Realtime.Player> GameOver;

		public Dictionary<Photon.Realtime.Player, Pawn> ActivePlayers { get; private set; }

		private void Awake()
		{
			if (!TryRegisterSingleton())
				return;
			ActivePlayers = new();

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

		public override void OnEnable()
		{
			base.OnEnable();
			Pawn.PlayerEliminated += OnPlayerEliminated;
			Pawn.PlayerJoined += OnPlayerJoined;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			Pawn.PlayerEliminated -= OnPlayerEliminated;
			Pawn.PlayerJoined -= OnPlayerJoined;
		}

		[PunRPC]
		public void GameStartRPC(PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Start can only be sent by master client");
			GameStart?.Invoke();
		}

		[PunRPC]
		public void GameOverRPC(Photon.Realtime.Player winningPlayer, PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
			GameOver?.Invoke(winningPlayer);

			if (PhotonNetwork.IsMasterClient)
				StartCoroutine(LeaveMatch());
		}

		private void TriggerGameOver(Photon.Realtime.Player winningPlayer)
		{
			const string GAME_OVER = nameof(GameOverRPC);
			photonView.RPC(GAME_OVER, RpcTarget.AllViaServer, winningPlayer);
		}

		private void OnPlayerEliminated(Pawn pawn)
		{
			ActivePlayers.Remove(pawn.Owner);
			if (PhotonNetwork.IsMasterClient && ActivePlayers.Count <= 1)
				TriggerGameOver(ActivePlayers.Single().Key);
		}

		private void OnPlayerJoined(Pawn pawn)
		{
			const string GAME_START = nameof(GameStartRPC);
			ActivePlayers.Add(pawn.Owner, pawn);
			if (PhotonNetwork.IsMasterClient && ActivePlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
				photonView.RPC(GAME_START, RpcTarget.AllViaServer);
		}

		IEnumerator LeaveMatch()
		{
			yield return new WaitForSeconds(2f); // TODO Test without this delay
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}