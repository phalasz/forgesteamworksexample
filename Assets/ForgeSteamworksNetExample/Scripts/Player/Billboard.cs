using UnityEngine;

namespace ForgeSteamworksNETExample.Player
{
	public class Billboard : MonoBehaviour
	{
		public bool alignNotLook = true;

		private Transform playerCam;

		private void Start()
		{
			playerCam = FindObjectOfType<Camera>().transform;
		}

		private void LateUpdate()
		{
			if (alignNotLook)
				transform.forward = playerCam.forward;
			else
				transform.LookAt(transform.position + playerCam.transform.rotation * Vector3.forward, playerCam.transform.rotation * Vector3.up);
		}
	}
}
