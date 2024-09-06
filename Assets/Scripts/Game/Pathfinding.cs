using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using static Game.Board;
using static Game.Board.BoardMask;

namespace Game
{
	/// <summary>
	/// Contains various extension functions related to <see cref="BoardMask"/>.
	/// </summary>
	public static class Pathfinding
	{
		/// <summary>
		/// Shows where an entity starting in <paramref name="position"/> can reach in <paramref name="steps"/> steps given the <paramref name="traversable"/> area.
		/// </summary>
		/// <param name="position">Position of the entity.</param>
		/// <param name="steps">How many steps the entity can take.</param>
		/// <param name="traversable">Mask detailing where the entity can reach.</param>
		/// <returns>Where the entity can reach.</returns>
		[BurstCompile]
		public static BoardMask GetTraversableArea(Position position, byte steps, BoardMask traversable) => GetTraversableArea(position.ToIndex(), steps, traversable);

		/// <summary>
		/// Shows where an entity starting in (<paramref name="centerX"/>, <paramref name="centerY"/>) can reach in <paramref name="steps"/> steps given the <paramref name="traversable"/> area.
		/// </summary>
		/// <param name="index">Position of the entity.</param>
		/// <param name="steps">How many steps the entity can take.</param>
		/// <param name="traversable">Mask detailing where the entity can reach.</param>
		/// <returns>Where the entity can reach.</returns>
		[BurstCompile]
		public static BoardMask GetTraversableArea(Vector2Int index, byte steps, BoardMask traversable)
		{
			BoardMask reach = new();
			reach[index] = true;
			while (steps > 0 && !reach.Empty())
			{
				reach = reach.Spread() & traversable;
				steps--;
			}
			return reach;
		}

		/// <param name="position">Position of the area center.</param>
		/// <param name="radius">Radius of the area.</param>
		/// <returns>The area centered in <paramref name="position"/> with a radius of <paramref name="radius"/>.</returns>
		public static BoardMask GetArea(Position position, byte radius) => GetArea(position.ToIndex(), radius);

		/// <param name="index">Position of the area center.</param>
		/// <param name="radius">Radius of the area.</param>
		/// <returns>The area centered in <paramref name="index"/> with a radius of <paramref name="radius"/>.</returns>
		[BurstCompile]
		public static BoardMask GetArea(Vector2Int index, byte radius)
		{
			BoardMask mask = new();
			int diameter = 1 + (radius << 1);
			int startX = index.x - radius;
			int endX = startX + diameter;
			startX = Math.Max(startX, 0);
			endX = Math.Min(endX, WIDTH);
			for (int x = startX; x < endX; x++)
				Column(x);
			return mask;

			[BurstCompile]
			void Column(int x)
			{
				int height = diameter - Math.Abs(index.x - x);
				int startY = index.y - (height >> 1) - ((index.x % 2) * (x % 2)) + (index.x % 2);
				int endY = startY + height;
				startY = Math.Max(startY, 0);
				endY = Math.Min(endY, HEIGHT);
				for (int y = startY; y < endY; y++)
					mask[new(x, y)] = true;
			}
		}

		/// <summary>
		/// Spreads <paramref name="mask"/> by 1 as if its bounds were extended.
		/// </summary>
		/// <param name="mask">The shape to spread.</param>
		/// <returns>The spread version of <paramref name="mask"/>.</returns>
		[BurstCompile]
		public static BoardMask Spread(this BoardMask mask)
		{
			if (mask.Empty() || mask == FULL)
				return mask;
			foreach (var bitPosition in mask)
				mask |= GetNeighbors(bitPosition);
			return mask;
		}

		/// <param name="position">Position of the tile.</param>
		/// <returns>The immediate neighbors of tile at <paramref name="position"/>.</returns>
		public static BoardMask GetNeighbors(Position position) => GetArea(position, 1);

		/// <param name="index">Position of the tile.</param>
		/// <returns>The immediate neighbors of tile at <paramref name="index"/>.</returns>
		public static BoardMask GetNeighbors(Vector2Int index) => GetArea(index, 1);

		/// <param name="index">Position of the tile.</param>
		/// <returns>A series of rings centered in <paramref name="index"/>.</returns>
		public static IEnumerable<BoardMask> GetRings(Vector2Int index)
		{
			BoardMask mask = new();
			byte radius = 1;
			mask[index] = true;
			while (!mask.Empty())
			{
				yield return mask;
				mask = GetArea(index, radius) & ~mask;
			}
		}
	}
}
