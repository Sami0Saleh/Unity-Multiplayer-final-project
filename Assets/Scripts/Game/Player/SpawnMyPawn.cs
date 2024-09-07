using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using static Utility;

namespace Game.Player
{
	public class SpawnMyPawn : MonoBehaviour
	{
		[SerializeField] private Pawn _pawnPrefab;
		private IEnumerable<Vector3> StartPositions => Board.StartingPositions.Select(p => Board.Instance.Grid.CellToWorld(Board.PositionToCell(p)));

        private void Start()
		{
			var start = GetStartPosition();
            PhotonNetwork.Instantiate(_pawnPrefab.name, start, Quaternion.identity);
			Destroy(this);
        }

		private Vector3 GetStartPosition()
		{
			return StartPositions.ElementAt(GetPlayerNumber(PhotonNetwork.LocalPlayer));
		}
	}
}
