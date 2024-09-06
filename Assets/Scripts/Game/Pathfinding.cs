using System;
using System.Collections.Generic;
using Unity.Burst;
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
		public static BoardMask GetTraversableArea(Position position, byte steps, BoardMask traversable)
		{
			(var x, var y) = position.ToIndex();
			return GetTraversableArea(x, y, steps, traversable);
		}

		/// <summary>
		/// Shows where an entity starting in (<paramref name="centerX"/>, <paramref name="centerY"/>) can reach in <paramref name="steps"/> steps given the <paramref name="traversable"/> area.
		/// </summary>
		/// <param name="centerX">Starting X coordinate of the entity.</param>
		/// <param name="centerY">Starting Y coordinate of the entity.</param>
		/// <param name="steps">How many steps the entity can take.</param>
		/// <param name="traversable">Mask detailing where the entity can reach.</param>
		/// <returns>Where the entity can reach.</returns>
		[BurstCompile]
		public static BoardMask GetTraversableArea(byte centerX, byte centerY, byte steps, BoardMask traversable)
		{
			BoardMask reach = new();
			reach[centerX, centerY] = true;
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
		public static BoardMask GetArea(Position position, byte radius)
		{
			(var x, var y) = position.ToIndex();
			return GetArea(x, y, radius);
		}

		/// <param name="centerX">X coordinate of the area center.</param>
		/// <param name="centerY">Y coordinate of the area center.</param>
		/// <param name="radius">Radius of the area.</param>
		/// <returns>The area centered in (<paramref name="centerX"/>, <paramref name="centerY"/>) with a radius of <paramref name="radius"/>.</returns>
		[BurstCompile]
		public static BoardMask GetArea(byte centerX, byte centerY, byte radius)
		{
			BoardMask mask = new();
			int diameter = 1 + (radius << 1);
			int startX = centerX - radius;
			int endX = startX + diameter;
			startX = Math.Max(startX, 0);
			endX = Math.Min(endX, WIDTH);
			for (byte x = (byte)startX; x < endX; x++)
				Column(x);
			return mask;

			[BurstCompile]
			void Column(byte x)
			{
				int height = diameter - Math.Abs(centerX - x);
				int startY = centerY - (height >> 1) - ((centerX % 2) * (x % 2)) + (centerX % 2);
				int endY = startY + height;
				startY = Math.Max(startY, 0);
				endY = Math.Min(endY, HEIGHT);
				for (byte y = (byte)startY; y < endY; y++)
					mask[x, y] = true;
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

		/// <param name="centerX">X coordinate of the tile.</param>
		/// <param name="centerY">Y coordinate of the tile.</param>
		/// <returns>The immediate neighbors of tile at (<paramref name="centerX"/>, <paramref name="centerY"/>).</returns>
		public static BoardMask GetNeighbors(byte centerX, byte centerY) => GetArea(centerX, centerY, 1);

		/// <param name="centerX">X coordinate of the rings center.</param>
		/// <param name="centerY">Y coordinate of the rings center.</param>
		/// <returns>A series of rings centered in (<paramref name="centerX"/>, <paramref name="centerY"/>).</returns>
		public static IEnumerable<BoardMask> GetRings(byte centerX, byte centerY)
		{
			BoardMask mask = new();
			byte radius = 1;
			mask[centerX, centerY] = true;
			while (!mask.Empty())
			{
				yield return mask;
				mask = GetArea(centerX, centerY, radius) & ~mask;
			}
		}
	}
}
