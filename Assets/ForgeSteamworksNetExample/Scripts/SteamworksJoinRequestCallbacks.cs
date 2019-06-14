using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;
using UnityEngine;

namespace ForgeSteamworksNETExample
{
	public class SteamworksJoinRequestCallbacks :MonoBehaviour
	{
		/// <summary>
		/// Reference to the multiplayer menu
		/// </summary>
		[SerializeField]
		private SteamworksMultiplayerMenu mpMenu;

		private Callback<GameLobbyJoinRequested_t> callbackLobbyJoinRequest;

		private void Awake()
		{
			callbackLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
		}

		private void OnDestroy()
		{
			callbackLobbyJoinRequest = null;
		}

		/// <summary>
		/// Handle the lobby join requests when already in game for an invite
		/// </summary>
		/// <param name="result"></param>
		private void OnLobbyJoinRequested(GameLobbyJoinRequested_t result)
		{
			// TODO: make sure join requests can be accepted if already playing.
			//       that will require setting the lobby id somewhere else and disconnecting from the game first.
			mpMenu.SetSelectedLobby(result.m_steamIDLobby);
			mpMenu.Connect();
		}
	}
}
