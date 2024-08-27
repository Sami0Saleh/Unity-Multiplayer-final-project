using System;
using Unity.Burst;
using static Game.Board;

namespace Game
{
	public static class Pathfinding
	{
		[BurstCompile]
		public static BoardMask GetArea(byte centerX, byte centerY, byte radius)
		{
			BoardMask mask = new();
			int diameter = 1 + (radius << 1);
			int startX = centerX - (diameter >> 1);
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
				int startY = centerY - (height >> 1) - ((centerX % 2) * (x % 2));
				int endY = startY + height;
				startY = Math.Max(startY, 0);
				endY = Math.Min(endY, HEIGHT);
				for (byte y = (byte)startY; y < endY; y++)
					mask[x, y] = true;
			}
		}
	}
}
