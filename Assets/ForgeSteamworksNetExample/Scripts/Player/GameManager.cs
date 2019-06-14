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
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				NetworkManager.Instance.Disconnect();

				SceneManager.LoadScene(0);
			}
		}
	}
}
