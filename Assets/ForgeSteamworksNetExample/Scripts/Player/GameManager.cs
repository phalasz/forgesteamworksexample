using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForgeSteamworksNETExample.Player
{
	public class GameManager : MonoBehaviour
	{
		private void Start()
		{
			NetworkManager.Instance.InstantiatePlayer();
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
