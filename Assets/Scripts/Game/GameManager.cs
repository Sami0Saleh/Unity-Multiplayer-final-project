using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using Game.Player;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
        private const float TIME_TO_LEAVE_GAME = 5f;
        public static GameManager Instance;

		public event UnityAction GameStart;
		public event UnityAction<PunPlayer> GameOver;

		public Dictionary<PunPlayer, Pawn> ActivePlayers { get; private set; }

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
			Pawn.PlayerJoined += OnPawnCreated;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			Pawn.PlayerEliminated -= OnPlayerEliminated;
			Pawn.PlayerJoined -= OnPawnCreated;
		}

		[PunRPC]
		private void GameStartRPC(PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Start can only be sent by master client");
			GameStart?.Invoke();
		}

		[PunRPC]
		private void GameOverRPC(PunPlayer winningPlayer, PhotonMessageInfo info)
		{
			Debug.Assert(info.Sender.IsMasterClient, "Game Over can only be sent by master client");
			GameOver?.Invoke(winningPlayer);

			if (PhotonNetwork.IsMasterClient)
				StartCoroutine(LeaveMatch(TIME_TO_LEAVE_GAME));
		}

		public void TriggerGameOver(PunPlayer winningPlayer)
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

		private void OnPawnCreated(Pawn pawn)
		{
			const string GAME_START = nameof(GameStartRPC);
			if (ActivePlayers.TryAdd(pawn.Owner, pawn) && PhotonNetwork.IsMasterClient && ActivePlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
				photonView.RPC(GAME_START, RpcTarget.AllViaServer);
		}

		IEnumerator LeaveMatch(float time)
		{
            yield return new WaitForSeconds(time); // TODO Test without this delay
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}

		public override void OnPlayerEnteredRoom(PunPlayer newPlayer)
		{
			if (!newPlayer.HasRejoined)
				return;
		}

		public override void OnLeftRoom()
		{
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}