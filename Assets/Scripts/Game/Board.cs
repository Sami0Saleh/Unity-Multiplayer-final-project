using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Tile = UnityEngine.GameObject;
using Unity.Burst;
using Game.Player;
using static Game.Board.BoardMask;

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

		public static IEnumerable<Position> StartingPositions
		{
			get
			{
				yield return BottomLeft();
				yield return TopRight();
				yield return TopLeft();
				yield return BottomRight();

				static Position BottomLeft() => new(new(0, 1));
				static Position TopRight() => new(new(WIDTH - 1, HEIGHT - 2));
				static Position TopLeft() => new(new(0, HEIGHT - 2));
				static Position BottomRight() => new(new(WIDTH - 1, 1));
			}
		}
		#region CELL_BOUNDS
		private const int X_MIN = Y_OFFSET;
		private const int X_MAX = -Y_OFFSET;
		private const int Y_MIN = X_OFFSET;
		private const int Y_MAX = -X_OFFSET;
		#endregion
		#endregion

		#region VARIABLES_AND_PROPERTIES
		public static Board Instance { get; private set; }

		[field: SerializeField] public Grid Grid { get; private set; }
		[field: SerializeField] public Transform TilesParent { get; private set; }
		[field: SerializeField] public Transform PawnsParent { get; private set; }
		[SerializeField] private Tile _tilePrefab;
		[SerializeField, HideInInspector] private BoardMask _initialBoardState;
		private Tile[] _tiles;

		public BoardMask CurrentBoardState { get; private set; }
		public BoardMask PawnPositions { get; private set; }
		public BoardMask TraversableArea => CurrentBoardState & ~PawnPositions;
		public IEnumerable<Tile> Tiles => _tiles;
		#endregion

		#region EVENTS
		public event UnityAction<IEnumerable<BoardMask.Position>> OnTilesRemoved;
		#endregion

		#region METHODS
		#region UNITY
		private void OnValidate()
		{
			_initialBoardState = GetInitialBoardMaskForTileCreation();

			static BoardMask GetInitialBoardMaskForTileCreation()
			{
				BoardMask mask = FULL;
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
					Position y = HEIGHT - 1;
					for (Position x = 1; x < WIDTH; x += 2)
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
				foreach (var bitNumber in _initialBoardState)
					CreateTile(bitNumber);
				CheckForValidInit(_initialBoardState);
			}

			Tile CreateTile(Position position)
			{
				var index = position.ToIndex();
				var cell = IndexToCell(index);
				var tile = Instantiate(_tilePrefab, Grid.CellToWorld(cell), Quaternion.identity, TilesParent);
				tile.name = $"Tile @ {index.x}, {index.y}";
				_tiles[position] = tile;
				CurrentBoardState |= position.ToMask();
				return tile;
			}

			void CheckForValidInit(BoardMask input) => Debug.Assert(CurrentBoardState == input, "CurrentBoardState must match the input from GetBoardMaskForTileCreation.");
		}

		private void OnEnable()
		{
			Pawn.PlayerJoined += OnPlayerJoined;
			Pawn.PlayerEliminated += OnPlayerEliminated;
		}

		private void OnDisable()
		{
			Pawn.PlayerJoined -= OnPlayerJoined;
			Pawn.PlayerEliminated -= OnPlayerEliminated;
		}
		#endregion

		#region GAMEPLAY
		private void OnPlayerJoined(Pawn pawn)
		{
			PawnPositions |= pawn.Position.ToMask();
			pawn.Movement.OnPawnMoved += OnPlayerMoved;
			pawn.Hammer.OnHammered += RemoveTile;
		}

		private void OnPlayerEliminated(Pawn pawn)
		{
			PawnPositions &= ~pawn.Position.ToMask();
			pawn.Movement.OnPawnMoved -= OnPlayerMoved;
			pawn.Hammer.OnHammered -= RemoveTile;
			if (CurrentBoardState.Contains(pawn.Position))
				RemoveTile(pawn.Position);
		}

		private void OnPlayerMoved(PawnMovement.PawnMovementEvent movementEvent)
		{
			RemoveTiles(movementEvent.AllStepsButLast);
			PawnPositions &= ~movementEvent.steps.First().ToMask();
			PawnPositions |= movementEvent.steps.Last().ToMask();
		}

		private void RemoveTile(Position position)
		{
			RemoveTiles(GetSingleTileEnumerable(position));

			static IEnumerable<Position> GetSingleTileEnumerable(byte toRemove)
			{
				yield return toRemove;
			}
		}

		private void RemoveTiles(IEnumerable<Position> toRemove)
		{
			var removedTilesMask = FromPositions(toRemove);
			OnTilesRemoved?.Invoke(toRemove);
			foreach (var tile in MaskToTiles(removedTilesMask))
				Destroy(tile);
			CurrentBoardState &= ~removedTilesMask;
		}
		#endregion

		#region INDEXING_AND_ENUMERATION
		public bool IsCellOnBoard(Vector3Int cell) => WithinBounds(cell) && CurrentBoardState.Contains(CellToPosition(cell));

		public bool WithinBounds(Vector3Int cell) => cell.x >= X_MIN && cell.x <= X_MAX && cell.y >= Y_MIN && cell.y <= Y_MAX;
		
		public Position WorldPositionToBitNumber(Vector3 worldPosition) => CellToPosition(Grid.WorldToCell(worldPosition));

		public static Vector3Int PositionToCell(Position position) => IndexToCell(position.ToIndex());

		public static Vector3Int IndexToCell(Vector2Int index) => new(index.y + Y_OFFSET, index.x + X_OFFSET);

		public static Vector2Int CellToIndex(Vector3Int cell) => new(cell.y - X_OFFSET, cell.x - Y_OFFSET);

		public static Position CellToPosition(Vector3Int cell) => new Position(CellToIndex(cell));

		public Tile PositionToTile(Position position) => _tiles[position];

		public IEnumerable<Tile> MaskToTiles(BoardMask mask)
		{
			mask &= CurrentBoardState;
			foreach (var position in mask)
				yield return PositionToTile(position);
		}
		#endregion
		#endregion

		public struct BoardMask : IEquatable<BoardMask>, IEnumerable<Position>
		{
			public const ulong FULL = ulong.MaxValue >> 1;

			private ulong _mask;

			#region FUNCTIONS
			#region OPERATORS
			public bool this[byte x, byte y]
			{
				readonly get => this[new(x, y)];
				set
				{
					Vector2Int index = new(x, y);
					if (value)
						_mask |= IndexToMask(index);
					else
						_mask &= ~IndexToMask(index);
				}
			}
			public bool this[Vector2Int index]
			{
				readonly get => (_mask & IndexToMask(index)) != 0;
				set
				{
					if (value)
						_mask |= IndexToMask(index);
					else
						_mask &= ~IndexToMask(index);
				}
			}

			public static implicit operator ulong(BoardMask mask) => mask._mask;

			public static implicit operator BoardMask(ulong mask) => new() { _mask = mask };
			#endregion

			#region CONVERSIONS
			public static ulong IndexToMask(Vector2Int index) => new Position(index).ToMask();

			[BurstCompile]
			public static BoardMask FromPositions(IEnumerable<Position> positions)
			{
				BoardMask mask = new();
				foreach (Position position in positions)
					mask |= position.ToMask();
				return mask;
			}

			public override readonly string ToString() => string.Format("0x{0:X}", _mask);
			#endregion

			#region CHECKS
			public readonly bool Empty() => _mask == 0;

			public readonly bool Contains(Position position) => (position.ToMask() & _mask) != 0;

			/// <returns>Whether <paramref name="other"/> is entirely contained within <see langword="this"/>.</returns>
			public readonly bool Contains(BoardMask other) => other.Equals(other & this);

			public readonly bool Equals(BoardMask other) => _mask == other._mask;
			#endregion

			#region ENUMERATION
			public readonly IEnumerator<Position> GetEnumerator()
			{
				for (Position p = 0; p <= MAX_NUMBER_OF_TILES; p++)
				{
					if (Contains(p))
						yield return p;
				}
			}

			readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public readonly IEnumerable<Vector2Int> Indices => this.Select(p => p.ToIndex());
			#endregion
			#endregion

			/// <summary>
			/// Represents a single position on a <see cref="BoardMask"/>.
			/// </summary>
			[BurstCompile]
			public struct Position : IEquatable<Position>
			{
				private byte _position;

				public static implicit operator byte(Position position) => position._position;

				public static implicit operator Position(byte position) => new() { _position = position };

				#region CONVERSIONS
				public Position(Vector2Int index) => _position = FromIndex(index);

				public readonly ulong ToMask() => (ulong)1 << _position;

				public readonly Vector2Int ToIndex() => new(_position % WIDTH, _position / WIDTH);

				public static Position FromIndex(Vector2Int index) => (Position)(index.x + index.y * WIDTH);
				#endregion

				public readonly bool Equals(Position other) => _position == other._position;

				public override readonly string ToString() => ToIndex().ToString();
			}
		}
	}
}