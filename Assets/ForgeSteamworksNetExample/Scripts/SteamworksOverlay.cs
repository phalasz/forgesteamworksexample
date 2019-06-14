using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;
using UnityEngine;

namespace ForgeSteamworksNETExample
{
	public class SteamworksOverlay :MonoBehaviour
	{
		/// <summary>
		/// Reference to the multiplayer menu
		/// </summary>
		[SerializeField]
		private SteamworksMultiplayerMenu mpMenu;

		private Callback<GameLobbyJoinRequested_t> callbackLobbyJoinRequest;

		private void Awake()
		{
			DontDestroyOnLoad(this);

			callbackLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
		}

		private void OnLobbyJoinRequested(GameLobbyJoinRequested_t result)
		{
			mpMenu.SetSelectedLobby(result.m_steamIDLobby);
			mpMenu.Connect();
		}
	}
}
