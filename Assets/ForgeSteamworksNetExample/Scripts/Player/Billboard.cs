using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace ForgeSteamworksNETExample.Player
{
	public class Billboard : MonoBehaviour
	{
		[SerializeField]
		private NetworkedPlayer player;

		[SerializeField]
		private bool alignNotLook = true;

		private Transform playerCam;

		private void Awake()
		{
			player.NetworkStartEvent += OnNetworkStarted;
		}

		private void OnDestroy()
		{
			player.NetworkStartEvent -= OnNetworkStarted;
		}

		private void OnNetworkStarted()
		{
			MainThreadManager.Run(() =>
			{
				playerCam = GameManager.localPlayer.GetComponentInChildren<Camera>().transform;
			});
		}

		private void LateUpdate()
		{
			if (playerCam == null)
				return;


			if (alignNotLook)
				transform.forward = playerCam.forward;
			else
				transform.LookAt(transform.position + playerCam.transform.rotation * Vector3.forward, playerCam.transform.rotation * Vector3.up);
		}
	}
}
