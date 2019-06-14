using Steamworks;
using UnityEngine;

namespace ForgeSteamworksNETExample
{
	public class SteamworksOverlay :MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
			{
				if (SteamManager.Initialized)
				{
					SteamFriends.ActivateGameOverlay("friends");
				}
			}
		}
	}
}
