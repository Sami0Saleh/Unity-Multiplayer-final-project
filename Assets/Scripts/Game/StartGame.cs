using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class StartGame : MonoBehaviourPun
	{
		[SerializeField] private Player _playerPrefab;
		[SerializeField] private List<Transform> _startPositions;

		private void Start()
		{
			var start = GetStartPosition();
			PhotonNetwork.Instantiate(_playerPrefab.name, start.position, start.rotation);
		}

		private Transform GetStartPosition()
		{
			return _startPositions[PhotonNetwork.LocalPlayer.ActorNumber]; // TODO Make sure this doesn't crash if a player leaves and another joins in Room Menu
		}
	}
}
