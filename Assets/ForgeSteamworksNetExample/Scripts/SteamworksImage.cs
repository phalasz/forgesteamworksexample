using System;
using UnityEngine;

namespace ForgeSteamworksNETExample
{
	public struct SteamworksImage
	{
		public uint Width;
		public uint Height;
		public byte[] imageData;

		public Color GetPixel(int x, int y)
		{
			if (x < 0 || x >= Width)
				throw new ArgumentOutOfRangeException("x", x, "Out of range");

			if (y < 0 || y >= Height)
				throw new ArgumentOutOfRangeException("y", y, "Out of range");

			var color = new Color();

			var i = (y * Width + x) * 4;

			color.r = imageData[i] / 255f;
			color.g = imageData[i + 1] / 255f;
			color.b = imageData[i + 2] / 255f;
			color.a = imageData[i + 3] / 255f;

			return color;
		}
	}
}
