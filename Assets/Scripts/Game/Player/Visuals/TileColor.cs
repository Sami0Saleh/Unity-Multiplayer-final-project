using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Game.Player.Cursor;

namespace Game.Player.Visuals
{
    public class TileColor : MonoBehaviour
    {
        [SerializeField] private Cursor _cursor;
        [SerializeField] private List<Material> _neutralMaterials;
		[SerializeField] private List<Material> _moveMaterials;
		[SerializeField] private List<Material> _hammerMaterials;

		private Board _board;
        private void Start() => _board = Board.Instance;
		private void OnEnable() => _cursor.StateChanged += OnChangeState;

		private void OnDisable() => _cursor.StateChanged -= OnChangeState;

		private void OnChangeState(State state)
        {
			ColorsTiles(State.Neutral);
			if (state != State.Neutral)
				ColorsTiles(state);
        }

		private void ColorsTiles(State state)
		{
			var tiles = GetTilesForState(state);
			var materials = GetMaterialsForState(state);
			foreach (var tile in tiles.Where(t => t != null))
				tile.Renderer.SetMaterials(materials);

			List<Material> GetMaterialsForState(State state)
			{
				return state switch
				{
					State.Move => _moveMaterials,
					State.Hammer => _hammerMaterials,
					_ => _neutralMaterials,
				};
			}

			IEnumerable<Tile> GetTilesForState(State state)
			{
				return state switch
				{
					State.Move => _board.MaskToTiles(_cursor.OwnerPawn.Movement.ReachableArea),
					State.Hammer => _board.MaskToTiles(_cursor.OwnerPawn.Hammer.HammerableArea),
					_ => _board.Tiles,
				};
			}
		}
    }
}