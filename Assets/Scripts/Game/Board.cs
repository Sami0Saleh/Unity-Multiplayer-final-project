using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Tile = UnityEngine.GameObject;

namespace Game
{
	public class Board : MonoBehaviour
	{
		#region CONSTS
		public const byte WIDTH = 9;
		public const byte HEIGHT = 7;
		public const byte MAX_NUMBER_OF_TILES = HEIGHT * WIDTH;
		private const int X_OFFSET = -WIDTH / 2;
		private const int Y_OFFSET = -HEIGHT / 2;
		public static BoardMask STARTING_POSITIONS =
			BoardMask.IndexToMask(0, 1) |
			BoardMask.IndexToMask(WIDTH - 1, 1) |
			BoardMask.IndexToMask(0, HEIGHT-2) |
			BoardMask.IndexToMask(WIDTH - 1, HEIGHT -2);
		#endregion

		#region VARIABLES_AND_PROPERTIES
		public static Board Instance;

		[field: SerializeField] public Grid Grid { get; private set; }
		[field: SerializeField] public Transform TilesParent { get; private set; }
		[field: SerializeField] public Transform PawnsParent { get; private set; }
		[SerializeField] private Tile _tilePrefab;
		[SerializeField, HideInInspector] private BoardMask _initialBoardState;
		private Tile[] _tiles;

		public BoardMask CurrentBoardState { get; private set; }
		public IEnumerable<Tile> Tiles => _tiles;
		#endregion

		#region EVENTS
		public event UnityAction<IEnumerable<byte>> OnTilesRemoved;
		#endregion

		#region METHODS
		#region UNITY
		private void OnValidate()
		{
			_initialBoardState = GetInitialBoardMaskForTileCreation();

			static BoardMask GetInitialBoardMaskForTileCreation()
			{
				BoardMask mask = BoardMask.FULL;
				RemoveCorners();
				RemoveOddsFromLastRow();
				return mask;

				void RemoveCorners()
				{
					mask[0, 0] = false;
					mask[WIDTH-1, 0] = false;
					mask[0, HEIGHT-1] = false;
					mask[WIDTH-1, HEIGHT-1] = false;
				}

				void RemoveOddsFromLastRow()
				{
					byte y = HEIGHT - 1;
					for (byte x = 1; x < WIDTH; x += 2)
						mask[x, y] = false;
				}
			}
		}

		private void Awake()
		{
			if (!RegisterSingleton())
				return;
			CreateTiles();

			bool RegisterSingleton()
			{
				bool canRegister = Instance == null;
				if (canRegister)
					Instance = this;
				return canRegister;
			}

			void CreateTiles()
			{
				_tiles = new Tile[MAX_NUMBER_OF_TILES];
				foreach ((var x, var y) in _initialBoardState.Indices)
					CreateTile(x, y);
				CheckForValidInit(_initialBoardState);
			}

			Tile CreateTile(byte x, byte y)
			{
				var cell = IndexToCell(x, y);
				var tile = Instantiate(_tilePrefab, Grid.CellToWorld(cell), Quaternion.identity, TilesParent);
				tile.name = $"Tile @ {x}, {y}";
				var bitNumber = BoardMask.IndexToBitNumber(x, y);
				_tiles[bitNumber] = tile;
				CurrentBoardState |= BoardMask.BitNumberToMask(bitNumber);
				return tile;
			}

			void CheckForValidInit(BoardMask input) => Debug.Assert(CurrentBoardState == input, "CurrentBoardState must match the input from GetBoardMaskForTileCreation.");
		}
		#endregion

		#region GAMEPLAY
		public void RemoveTiles(IEnumerable<byte> toRemove)
		{
			var removedTilesMask = BoardMask.BitNumbersToMask(toRemove);
			CurrentBoardState &= ~removedTilesMask;
			OnTilesRemoved?.Invoke(toRemove);
			foreach (var tile in TilesFromMask(removedTilesMask))
				Destroy(tile);
		}
		#endregion

		#region INDEXING_AND_ENUMERATION
		public static Vector3Int IndexToCell(byte x, byte y) => new(y + Y_OFFSET, x + X_OFFSET);

		public (byte, byte) CellToIndex(Vector3Int cell) => ((byte)(cell.y - Y_OFFSET), (byte)(cell.x - X_OFFSET));

		public Tile TileFromBitNumber(byte bit) => _tiles[bit];

		public IEnumerable<Tile> TilesFromMask(BoardMask mask)
		{
			mask &= CurrentBoardState;
			foreach (var bit in mask)
				yield return TileFromBitNumber(bit);
		}
		#endregion
		#endregion

		public struct BoardMask : IEquatable<BoardMask>, IEnumerable<byte>
		{
			public const ulong FULL = ulong.MaxValue >> 1;

			private ulong _mask;

			#region FUNCTIONS
			#region OPERATORS
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
			#endregion

			#region CONVERSIONS
			public static ulong IndexToMask(byte x, byte y) => BitNumberToMask(IndexToBitNumber(x, y));

			public static ulong BitNumberToMask(byte bitNumber) => (ulong)1 << bitNumber;

			public static (byte, byte) BitNumberToIndex(byte bitNumber) => ((byte)(bitNumber % WIDTH), (byte)(bitNumber / WIDTH));

			public static byte IndexToBitNumber(byte x, byte y) => (byte)(x + y * WIDTH);

			public static BoardMask BitNumbersToMask(IEnumerable<byte> steps)
			{
				BoardMask mask = new();
				foreach (var step in steps)
					mask |= BitNumberToMask(step);
				return mask;
			}
			#endregion

			#region CHECKS
			public readonly bool Empty() => _mask == 0;

			public readonly bool Contains(byte bitNumber) => (BitNumberToMask(bitNumber) & _mask) != 0;

			public readonly bool Equals(BoardMask other) => _mask == other._mask;
			#endregion

			#region ENUMERATION
			public readonly IEnumerator<byte> GetEnumerator()
			{
				for (byte b = 0; b <= MAX_NUMBER_OF_TILES; b++)
				{
					if (Contains(b))
						yield return b;
				}
			}

			readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public readonly IEnumerable<(byte, byte)> Indices => this.Select(BitNumberToIndex);
			#endregion
			#endregion
		}
	}
}