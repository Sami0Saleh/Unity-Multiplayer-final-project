using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static Utility;

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
			return _startPositions[GetPlayerNumber(PhotonNetwork.LocalPlayer)];
		}
	}
}
