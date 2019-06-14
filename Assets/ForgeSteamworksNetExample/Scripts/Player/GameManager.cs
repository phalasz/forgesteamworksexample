using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace ForgeSteamworksNETExample.Player
{
	public class GameManager : MonoBehaviour
	{
		private void Start()
		{
			NetworkManager.Instance.InstantiatePlayer();
		}
	}
}
