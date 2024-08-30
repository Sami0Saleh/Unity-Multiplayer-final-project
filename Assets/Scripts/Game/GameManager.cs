using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Game.Player;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
		private const string GAME_OVER = nameof(OnGameOver);
		public static GameManager Instance;

		public event UnityAction<Photon.Realtime.Player> GameOver;

		public Dictionary<Photon.Realtime.Player, Pawn> ActivePlayers { get; private set; }

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
		public void OnGameOver(Photon.Realtime.Player winningPlayer, PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
			GameOver?.Invoke(winningPlayer);

			if (PhotonNetwork.IsMasterClient)
				StartCoroutine(LeaveMatch());
		}

		public void TriggerGameOver(Photon.Realtime.Player winningPlayer)
		{
			photonView.RPC(GAME_OVER, RpcTarget.All, winningPlayer);
		}

		private void OnPlayerEliminated(Pawn pawn)
		{
			if (!PhotonNetwork.IsMasterClient)
				return;
			ActivePlayers.Remove(pawn.ThisPlayer);
			if (ActivePlayers.Count <= 1)
				TriggerGameOver(pawn.ThisPlayer);
		}

		private void OnPlayerJoined(Pawn pawn) => ActivePlayers.Add(pawn.ThisPlayer, pawn);

		IEnumerator LeaveMatch()
		{
			yield return new WaitForSeconds(2f); // TODO Test without this delay
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}