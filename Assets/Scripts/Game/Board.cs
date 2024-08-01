using System;
using UnityEngine;

namespace Game
{
	public class Board : MonoBehaviour
	{
		public const byte WIDTH = 7;
		public const byte HEIGHT = 9;
		public const byte MAX_NUMBER_OF_TILES = HEIGHT * WIDTH;

		[SerializeField] private GameObject _tilePrefab;
		[SerializeField] private Grid _grid;
		[SerializeField] private Transform _tilesParent;
		[SerializeField, HideInInspector] private GameObject[] _tiles;
		[field: SerializeField, HideInInspector] public BoardMask CurrentBoardState { get; private set; }

		private void OnValidate()
		{
			ValidateCurrentBoardState();

			void ValidateCurrentBoardState()
			{
				CurrentBoardState = 0;
				for (byte n = 0; n < _tiles.Length; n++)
				{
					if (_tiles[n] != null)
						CurrentBoardState |= BoardMask.BitNumberToMask(n);
				}
			}
		}

		[ContextMenu("Recreate Tiles")]
		private void ReCreateTiles()
		{
			_tilesParent.DestroyAllChildrenImmediate();
			CreateTiles(WIDTH, HEIGHT);

			void CreateTiles(byte width, byte height)
			{
				_tiles = new GameObject[MAX_NUMBER_OF_TILES];
				for (byte y = 0; y < height; y++)
				{
					for (byte x = 0; x < width; x++)
					{
						CreateTile(x, y);
					}
				}
			}

			GameObject CreateTile(byte x, byte y)
			{
				var index = new Vector3Int(x, y, 0);
				var tile = Instantiate(_tilePrefab, _grid.CellToWorld(index), Quaternion.identity, _tilesParent);
				tile.name = $"Tile @ {x}, {y}";
				var bitNumber = BoardMask.IndexToBitNumber(x, y);
				_tiles[bitNumber] = tile;
				CurrentBoardState |= BoardMask.BitNumberToMask(bitNumber);
				return tile;
			}
		}

		[Serializable]
		public struct BoardMask : IEquatable<BoardMask>
		{
			[SerializeField] private ulong _mask;

			public bool this[byte x, byte y]
			{
				readonly get => (_mask & IndexToMask(x, y)) != 0;
				set
				{
					if (value)
						_mask |= IndexToMask(x, y);
					else
						_mask &= ~IndexToMask(x, y);
				}
			}

			public static implicit operator ulong(BoardMask mask) => mask._mask;

			public static implicit operator BoardMask(ulong mask) => new() { _mask = mask };

			public static ulong IndexToMask(byte x, byte y) => BitNumberToMask(IndexToBitNumber(x, y));

			public static ulong BitNumberToMask(byte bitNumber) => (ulong)1 << bitNumber;

			public static byte IndexToBitNumber(byte x, byte y) => (byte)(x + y * WIDTH);

			public readonly bool Equals(BoardMask other) => _mask == other._mask;
		}
	}
}