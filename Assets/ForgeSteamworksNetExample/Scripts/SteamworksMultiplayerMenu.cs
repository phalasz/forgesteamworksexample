using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Lobby;
using BeardedManStudios.SimpleJSON;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ForgeSteamworksNETExample
{
	public class SteamworksMultiplayerMenu : MonoBehaviour
	{
		/// <summary>
		/// Flag to indicate that Forge is in the process of connecting to a server/lobby
		/// </summary>
		public bool IsConnecting { get; private set; }

		public bool DontChangeSceneOnConnect = false;
		public bool connectUsingMatchmaking = false;
		public bool useMainThreadManagerForRPCs = true;

		public GameObject networkManager = null;
		public GameObject[] ToggledButtons;

		[Header("Server information")]
		public int maximumNumberOfPlayers = 5;
		public string gameId = "forgeGame";
		public string type = "Deathmatch";
		public string mode = "Teams";
		public string comment = "Demo comment...";

		[SerializeField]
		private SteamAvatar playerAvatar;

		[SerializeField]
		private TMP_InputField serverName;

		private NetworkManager mgr = null;
		private NetWorker server;

		/// <summary>
		/// The Steam ID of the selected lobby to join.
		/// </summary>
		/// <remarks>This value is set by the join menu when a server in the server list is clicked</remarks>
		private CSteamID selectedLobby;

		private List<Button> _uiButtons = new List<Button>();
		private bool _matchmaking = false;

		private void Start()
		{
#if !STEAMWORKS
			Debug.LogError("Missing STEAMWORKS define. This menu will not work without it");
			throw new SystemException("Missing STEAMWORKS define. This menu will not work without it");
#endif

			SteamAPI.Init();

			GetPlayerSteamInformation();

			for (int i = 0; i < ToggledButtons.Length; ++i)
			{
				Button btn = ToggledButtons[i].GetComponent<Button>();
				if (btn != null)
					_uiButtons.Add(btn);
			}

			if (useMainThreadManagerForRPCs)
				Rpc.MainThreadRunner = MainThreadManager.Instance;
		}

		/// <summary>
		/// Sets the lobby to be joined when clicking the connect button.
		/// Usually called by the <see cref="JoinMenu"/> when a server list item is clicked
		/// </summary>
		/// <param name="steamId"></param>
		public void SetSelectedLobby(CSteamID steamId)
		{
			selectedLobby = steamId;
		}

		/// <summary>
		/// Handle the connecting the selected lobby/server
		/// </summary>
		public void Connect()
		{
			SetToggledButtons(false);
			IsConnecting = true;

			if (connectUsingMatchmaking)
			{
				// Add custom matchmaking logic here.
				// eg.: pick a random lobby from the list of lobbies for the game
				return;
			}

			// Need to select a lobby first.
			if (selectedLobby == CSteamID.Nil)
				return;

			NetWorker client;

			client = new SteamP2PClient();
			((SteamP2PClient)client).Connect(selectedLobby);

			// Steamworks API calls are async so we need to delay the rest of the networker setup until
			// the local user joins the selected lobby.
			client.bindSuccessful += (networker) => {
				MainThreadManager.Run(() =>
				{
					Connected(client);
				});
			};

			client.bindFailure += sender =>
			{
				MainThreadManager.Run(() =>
				{
					ResetButtonsOnFailedConnection();
				});
			};

			client.disconnected += sender =>
			{
				MainThreadManager.Run(() =>
				{
					ResetButtonsOnFailedConnection();
				});
			};
		}

		/// <summary>
		/// Handle setting up a host. Called by the host button.
		/// </summary>
		public void Host()
		{
			// Currently there is a bug in the SteamP2PServer code where the lobby max member count is hard coded to be 5.
			// Until a fix is in place please change line 186 of the SteamP2PServer to read
			//    `m_CreateLobbyResult = SteamMatchmaking.CreateLobby(lobbyType, MaxConnections);`
			server = new SteamP2PServer(maximumNumberOfPlayers);

			// Don't yet have a way to invite players to lobby. Until then all hosts are set to be public
			//((SteamP2PServer)server).Host(SteamUser.GetSteamID(), isPrivateLobby ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic, OnLobbyReady);
			((SteamP2PServer)server).Host(SteamUser.GetSteamID(), ELobbyType.k_ELobbyTypePublic, OnLobbyReady);

			server.playerTimeout += (player, sender) => { Debug.Log("Player " + player.NetworkId + " timed out"); };

			Connected(server);
		}

		/// <summary>
		/// Setup to run after the <see cref="NetWorker"/> has connected
		/// </summary>
		/// <param name="networker"></param>
		public void Connected(NetWorker networker)
		{
			if (!networker.IsBound)
			{
				Debug.LogError("NetWorker failed to bind");
				return;
			}

			if (mgr == null && networkManager == null)
			{
				Debug.LogWarning("A network manager was not provided, generating a new one instead");
				networkManager = new GameObject("Network Manager");
				mgr = networkManager.AddComponent<NetworkManager>();
			} else if (mgr == null)
				mgr = Instantiate(networkManager).GetComponent<NetworkManager>();


			mgr.Initialize(networker);

			if (networker is IServer)
			{
				if (!DontChangeSceneOnConnect)
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
				else
					NetworkObject.Flush(networker); //Called because we are already in the correct scene!
			}
		}

		/// <summary>
		/// Called when failed to connect to a lobby.
		/// </summary>
		private void ResetButtonsOnFailedConnection()
		{
			// Only try to re-enable buttons and clicks if we are receiving a disconnect before
			// switching to the game scene
			if (IsConnecting)
			{
				SetToggledButtons(true);
				IsConnecting = false;
			}
		}

		/// <summary>
		/// Toggle button interaction
		/// </summary>
		/// <param name="value"></param>
		private void SetToggledButtons(bool value)
		{
			for (int i = 0; i < _uiButtons.Count; ++i)
				_uiButtons[i].interactable = value;
		}

		private void OnApplicationQuit()
		{
			if (server != null) server.Disconnect(true);
		}

		/// <summary>
		/// Setup the local user's steam avatar in the top left corner of the menu
		/// </summary>
		private void GetPlayerSteamInformation()
		{
			if (SteamManager.Initialized) {
				if (playerAvatar == null)
					playerAvatar = GetComponentInChildren<SteamAvatar>();

				if (playerAvatar == null)
					return;

				playerAvatar.Initialize(SteamUser.GetSteamID());
			}
		}

		/// <summary>
		/// Callback used when a host successfully created a lobby.
		/// Sets lobby metadata that is used on the server browser.
		/// </summary>
		private void OnLobbyReady()
		{
			// If the host has not set a server name then let's use his/her name instead to name the lobby
			var personalName = SteamFriends.GetPersonaName();
			var gameName = serverName.text == "" ? $"{personalName}'s game" : serverName.text;

			var lobbyId = ((SteamP2PServer) server).LobbyID;

			// Set the name of the lobby
			SteamMatchmaking.SetLobbyData(lobbyId, "name", gameName);

			// Set the unique id of our game so the server list only gets the games with this id
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameId", gameId);

			// Set all other game information
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameType", type);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameMode", mode);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameDesc", comment);
		}
	}
}
