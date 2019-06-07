using System;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace ForgeSteamworksNETExample
{
	public enum AvatarSize
	{
		Small,
		Medium,
		Large
	}

	public class SteamAvatar : MonoBehaviour
	{
		public RawImage avatarImage;

		[SerializeField]
		private Texture2D fallbackImage;

		private void Awake()
		{
			if (avatarImage == null)
			{
				avatarImage = GetComponentInChildren<RawImage>();
			}
		}

		public void Initialize(CSteamID steamId)
		{
			if (SteamManager.Initialized)
			{
				avatarImage.texture = GetAvatar(steamId);
			}
		}

		public Texture2D GetAvatar(CSteamID steamId, AvatarSize size = AvatarSize.Medium)
		{
			SteamworksImage image;
			try
			{
				image = GetAvatarFromStreamApi(size, steamId);
			}
			catch(SystemException e)
			{
				return fallbackImage;
			}

			var texture = new Texture2D((int)image.Width, (int)image.Height);
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					var pixel = image.GetPixel(x, y);

					texture.SetPixel(x, (int)image.Height - y, pixel);
				}
			}

			texture.Apply();

			return texture;
		}

		/// <summary>
		/// Get image data from the Steam Api
		/// </summary>
		/// <param name="size">The image size to get from Steam</param>
		/// <param name="steamId">The steam id of the user to get the image for</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="SystemException"></exception>
		public SteamworksImage GetAvatarFromStreamApi(AvatarSize size, CSteamID steamId)
		{
			int imageHandle;
			switch (size)
			{
				case AvatarSize.Small:
					imageHandle = SteamFriends.GetSmallFriendAvatar(steamId);
					break;
				case AvatarSize.Medium:
					imageHandle = SteamFriends.GetMediumFriendAvatar(steamId);
					break;
				case AvatarSize.Large:
					imageHandle = SteamFriends.GetLargeFriendAvatar(steamId);
					break;
				default:
					throw new ArgumentException("Unknown Steam Avatar size!");
			}

			if (imageHandle == -1 || imageHandle == 0)
				throw new SystemException("Invalid Steamworks image handle");

			var image = new SteamworksImage();


			if (!SteamUtils.GetImageSize(imageHandle, out image.Width, out image.Height))
				throw new SystemException("Couldn't get image size from Steamworks api");

			uint imageSize = image.Width * image.Height * 4;

			var buffer = new byte[imageSize];
			if (!SteamUtils.GetImageRGBA(imageHandle, buffer, (int) imageSize))
				throw new SystemException("Couldn't get image data  from Steamworks api");

			image.imageData = new byte[imageSize];
			Array.Copy(buffer, 0, image.imageData, 0, imageSize);

			return image;
		}
	}
}
