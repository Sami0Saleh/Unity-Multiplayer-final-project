using UnityEngine;
using Game;

namespace Test
{
	public class PathfindingTest : MonoBehaviour
	{
		[SerializeField] private byte _x, _y;
		[SerializeField] private byte _radius = 1;
		[SerializeField] private Color _color = Color.red;

		[ContextMenu("ShowRadius")]
		private void ShowRadius()
		{
			foreach (var tile in Board.Instance.MaskToTiles(Pathfinding.GetArea(_x, _y, _radius)))
				tile.GetComponentInChildren<MeshRenderer>().material.color = _color;
		}
	}
}
