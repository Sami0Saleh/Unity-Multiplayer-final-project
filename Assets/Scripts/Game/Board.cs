using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tile = UnityEngine.GameObject;

namespace Game
{
	public class Board : MonoBehaviour
	{
		public const byte WIDTH = 9;
		public const byte HEIGHT = 7;
		public const byte MAX_NUMBER_OF_TILES = HEIGHT * WIDTH;
		private const int X_OFFSET = -WIDTH / 2;
		private const int Y_OFFSET = -HEIGHT / 2;
		public static BoardMask STARTING_POSITIONS = BoardMask.IndexToMask(1, 0) | BoardMask.IndexToMask(1, 8) | BoardMask.IndexToMask(6, 0) | BoardMask.IndexToMask(6, 8);

		[SerializeField] private Tile _tilePrefab;
		[SerializeField] private Grid _grid;
		[SerializeField] private Transform _tilesParent;
		private Tile[] _tiles;

		public IEnumerable<Tile> Tiles => TilesFromMask(CurrentBoardState);
		public BoardMask CurrentBoardState { get; private set; }

		private void Awake()
		{
			CreateTiles(WIDTH, HEIGHT);

			void CreateTiles(byte width, byte height)
			{
				_tiles = new Tile[MAX_NUMBER_OF_TILES];
				for (byte y = 0; y < height; y++)
				{
					for (byte x = 0; x < width; x++)
					{
						CreateTile(x, y);
					}
				}
			}

			Tile CreateTile(byte x, byte y)
			{
				var cell = IndexToCell(x, y);
				var tile = Instantiate(_tilePrefab, _grid.CellToWorld(cell), Quaternion.identity, _tilesParent);
				tile.name = $"Tile @ {x}, {y}";
				var bitNumber = BoardMask.IndexToBitNumber(x, y);
				_tiles[bitNumber] = tile;
				CurrentBoardState |= BoardMask.BitNumberToMask(bitNumber);
				return tile;
			}
		}

		public static Vector3Int IndexToCell(byte x, byte y) => new(y + Y_OFFSET, x + X_OFFSET);

		public (byte, byte) CellToIndex(Vector3Int cell) => ((byte)(cell.y - Y_OFFSET), (byte)(cell.x - X_OFFSET));

		public IEnumerable<Tile> TilesFromMask(BoardMask mask)
		{
			foreach ((var x, var y) in mask)
				yield return _tiles[BoardMask.IndexToBitNumber(x, y)];
		}

		public struct BoardMask : IEquatable<BoardMask>, IEnumerable<(byte, byte)>
		{
			public const ulong FULL = ulong.MaxValue;

			private ulong _mask;

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

			public readonly IEnumerator<(byte, byte)> GetEnumerator() // TODO Improve efficiency
			{
				for (byte y = 0; y < HEIGHT; y++)
				{
					for (byte x = 0; x < WIDTH; x++)
					{
						if ((_mask & IndexToMask(x, y)) != 0)
							yield return (x, y);
					}
				}
			}

			readonly IEnumerator IEnumerable.GetEnumerator()
			{
				IEnumerator<(byte, byte)> e = GetEnumerator();
				return e;
			}
		}
	}
}