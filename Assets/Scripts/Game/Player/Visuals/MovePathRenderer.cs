using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Game.Board.BoardMask;

namespace Game.Player.Visuals
{
	public class MovePathRenderer : MonoBehaviour
	{
		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private Pawn _pawn;
		[SerializeField] private LineRenderer _lineRenderer;
		private readonly Vector3[] _path = new Vector3[PawnMovement.PawnMovementEvent.MAX_ELEMENTS_IN_STEPS];

		private void Start()
		{
			if (_colorConfig.TryGetMaterial(_pawn.Owner, out var profile))
				_lineRenderer.SetSharedMaterials(new List<Material>() {profile.lineMaterial});
			_pawn.Cursor.StateChanged += OnStateChanged;
		}

		private void OnDestroy() => _pawn.Cursor.StateChanged -= OnStateChanged;

		private void OnEnable()
		{
			_lineRenderer.enabled = true;
			_pawn.Movement.OnPathChanged += OnPathChanged;
		}

		private void OnDisable()
		{
			ResetLineRenderer();
			_lineRenderer.enabled = false;
			_pawn.Movement.OnPathChanged -= OnPathChanged;

			void ResetLineRenderer()
			{
				_lineRenderer.positionCount = 0;
				_lineRenderer.SetPositions(Array.Empty<Vector3>());
			}
		}

		private void OnStateChanged(Cursor.State state) => enabled = state == Cursor.State.Move;

		private void OnPathChanged(IEnumerable<Position> path)
		{
			_lineRenderer.positionCount = path.Count();
			_lineRenderer.SetPositions(UpdatePath());

			Vector3[] UpdatePath()
			{
				var board = Board.Instance;
				int index = 0;
				Array.Clear(_path, index, _path.Length);
				foreach (var position in path)
					_path[index++] = board.Grid.CellToWorld(Board.PositionToCell(position));
				return _path;
			}
		}
	}
}