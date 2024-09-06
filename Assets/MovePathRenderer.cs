using System;
using System.Collections.Generic;
using UnityEngine;
using static Game.Board.BoardMask;

namespace Game.Player.Visuals
{
	public class MovePathRenderer : MonoBehaviour
	{
		[SerializeField] private PawnMovement _movement;
		[SerializeField] private LineRenderer _lineRenderer;
		private readonly Vector3[] _path = new Vector3[PawnMovement.PawnMovementEvent.MAX_ELEMENTS_IN_STEPS];

		private void OnEnable()
		{
			_movement.OnPathChanged += OnPathChanged;
		}

		private void OnDisable()
		{
			_movement.OnPathChanged -= OnPathChanged;
		}

		private void OnPathChanged(IEnumerable<Position> path)
		{
			_lineRenderer.SetPositions(UpdatePath());

			Vector3[] UpdatePath()
			{
				var board = Board.Instance;
				int index = 0;
				Array.Clear(_path, index, _path.Length);
				foreach (var position in path)
					_path[index++] = board.PositionToTile(position).transform.position;
				return _path;
			}
		}
	}
}