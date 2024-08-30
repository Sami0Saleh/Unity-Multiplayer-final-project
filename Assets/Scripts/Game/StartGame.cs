using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using static Utility;

namespace Game
{
	public class StartGame : MonoBehaviour
	{
		[SerializeField] private Player.Pawn _pawnPrefab;
		private IEnumerable<GameObject> StartPositions => Board.Instance.TilesFromMask(Board.STARTING_POSITIONS);

        private void Start()
		{
			var start = GetStartPosition();
            PhotonNetwork.Instantiate(_pawnPrefab.name, start.position, start.rotation);
			Destroy(this);
        }

		private Transform GetStartPosition()
		{
			return StartPositions.ElementAt(GetPlayerNumber(PhotonNetwork.LocalPlayer)).transform;
		}
	}
}
