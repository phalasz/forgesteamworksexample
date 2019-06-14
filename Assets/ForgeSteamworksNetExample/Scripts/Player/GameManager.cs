using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForgeSteamworksNETExample.Player
{
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// Reference to the local client's player character
		/// </summary>
		public static NetworkedPlayer localPlayer;

		private void Start()
		{
			localPlayer = NetworkManager.Instance.InstantiatePlayer() as NetworkedPlayer;

			if (!NetworkManager.Instance.IsServer)
				return;

			NetworkManager.Instance.Networker.playerDisconnected += (player, networker) =>
			{
				MainThreadManager.Run((() =>
				{
					List<NetworkObject> toDelete = new List<NetworkObject>();
					foreach (var obj in networker.NetworkObjectList)
					{
						if (obj.Owner == player)
						{
							toDelete.Add(obj);
						}
					}

					if (toDelete.Count > 0)
					{
						for (int i = toDelete.Count - 1; i >= 0; i--)
						{
							networker.NetworkObjectList.Remove(toDelete[i]);
							toDelete[i].Destroy();
						}
					}
				}));
			};
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				NetworkManager.Instance.Disconnect();

				SceneManager.LoadScene(0);
			}
		}

		private void OnApplicationQuit()
		{
			NetworkManager.Instance.Disconnect();
		}
	}
}
