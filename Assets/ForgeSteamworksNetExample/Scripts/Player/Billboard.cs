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

		private void LateUpdate()
		{
			if (GameManager.localPlayer == null ||  GameManager.localPlayer.PlayerCamera == null)
				return;

			if (playerCam == null)
			{
				playerCam = GameManager.localPlayer.PlayerCamera.transform;
			}

			if (alignNotLook)
				transform.forward = playerCam.forward;
			else
				transform.LookAt(transform.position + playerCam.transform.rotation * Vector3.forward, playerCam.transform.rotation * Vector3.up);
		}
	}
}
