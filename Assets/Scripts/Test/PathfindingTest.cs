using UnityEngine;
using DG.Tweening;
using Game;

#if UNITY_EDITOR
namespace Test
{
	public class PathfindingTest : MonoBehaviour
	{
		[SerializeField] private byte _x, _y;
		[SerializeField] private byte _radius = 1;

		[ContextMenu("Select")]
		private void Select()
		{
			foreach (var tile in Board.Instance.MaskToTiles(Pathfinding.GetArea(_x, _y, _radius)))
				tile.transform.DOShakeScale(1f);
		}
	}
}
#endif
