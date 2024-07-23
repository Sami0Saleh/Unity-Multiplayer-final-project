using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

namespace Game
{
	public class GameManager : MonoBehaviourPunCallbacks
	{
		private const int MENU_SCENE_INDEX = 0;
		private const string GAME_OVER = nameof(OnGameOver);
		public static GameManager Instance;

		public event UnityAction<int> GameOver;

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
			_activePlayers.Remove(player);
			if (_activePlayers.Count <= 1)
				TriggerGameOver(_activePlayers.Single().ThisPlayer.ActorNumber);
		}

		private void OnPlayerJoined(PlayerCharacter player) => _activePlayers.Add(player);

		IEnumerator LeaveMatch()
		{
			yield return new WaitForSeconds(2f); // TODO Test without this delay
			PhotonNetwork.DestroyAll();
			PhotonNetwork.LoadLevel(MENU_SCENE_INDEX);
		}
	}
}