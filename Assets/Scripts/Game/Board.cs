using UnityEngine;

namespace Game
{
	public class Board : MonoBehaviour
	{
		const int WIDTH = 9;
		const int HEIGHT = 7;

		[SerializeField] private GameObject _tilePrefab;
		[SerializeField] private Transform _tilesParent;
		[SerializeField] private Grid _grid;

		[ContextMenu("Create Tiles")]
		private void CreateTiles()
		{
			for (int x = 0; x < WIDTH; x++)
			{
				for (int y = 0; y < HEIGHT; y++)
				{
					var index = new Vector3Int(x, y, 0);
					Object.Instantiate(_tilePrefab, _grid.CellToWorld(index), Quaternion.identity, _tilesParent);
				}
			}
		}
	}
}