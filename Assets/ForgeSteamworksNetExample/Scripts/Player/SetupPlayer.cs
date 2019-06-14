using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using Steamworks;
using UnityEngine;

namespace ForgeSteamworksNETExample.Player
{
	public class SetupPlayer : MonoBehaviour
	{
		/// <summary>
		/// The <see cref="NetworkedPlayer"/> to setup
		/// </summary>
		[SerializeField]
		private NetworkedPlayer player;

		/// <summary>
		/// The player's avatar that is displayed above the character model
		/// </summary>
		[SerializeField]
		private SteamAvatar avatar;

		private void Awake()
		{
			player.NetworkStartEvent += OnNetworkStarted;
			player.SetupPlayerEvent += OnSetupPlayerCalled;
		}

		private void OnDestroy()
		{
			player.NetworkStartEvent -= OnNetworkStarted;
			player.SetupPlayerEvent -= OnSetupPlayerCalled;
		}

		private void OnNetworkStarted()
		{
			if (player.networkObject.IsOwner && SteamManager.Initialized)
			{
				var steamId = SteamUser.GetSteamID();

				player.networkObject.SendRpc(PlayerBehavior.RPC_SETUP_PLAYER, Receivers.AllBuffered, steamId.m_SteamID);
			}
		}

		private void OnSetupPlayerCalled(RpcArgs args)
		{
			var steamId = args.GetNext<ulong>();
			player.SetSteamId(args.GetNext<ulong>());

			avatar.Initialize((CSteamID)steamId, AvatarSize.Small);
		}
	}
}
