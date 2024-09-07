using UnityEngine;
using DG.Tweening;
using Game;

#if UNITY_EDITOR
namespace Test
{
	public class Pathfinding : MonoBehaviour
	{
		[SerializeField] private byte _x, _y;
		[SerializeField] private byte _radius = 1;

		[ContextMenu("Select")]
		private void Select()
		{
			foreach (var tile in Board.Instance.MaskToTiles(Game.Pathfinding.GetArea(new Vector2Int(_x, _y), _radius)))
				tile.transform.DOShakeScale(1f);
		}
	}
}
#endif
