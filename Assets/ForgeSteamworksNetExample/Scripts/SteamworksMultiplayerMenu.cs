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
		public Toggle privateGameToggle;
		public bool DontChangeSceneOnConnect = false;
		public bool connectUsingMatchmaking = false;
		public bool useMainThreadManagerForRPCs = true;
		public bool useInlineChat = false;

		public int maximumNumberOfPlayers = 5;

		public GameObject networkManager = null;
		public GameObject[] ToggledButtons;

		[Header("Game information for server browser")]
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

		private bool isPrivateLobby;

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

			isPrivateLobby = privateGameToggle.isOn;

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
		/// </summary>
		/// <param name="steamId"></param>
		public void SetSelectedLobby(CSteamID steamId)
		{
			this.selectedLobby = steamId;
		}

		public void Connect()
		{
			if (connectUsingMatchmaking)
			{
				// Add custom matchmaking logic here.
				// eg.: pick a random lobby from the list of lobbies for the game
				return;
			}

			if (selectedLobby == CSteamID.Nil)
				return;

			NetWorker client;

			client = new SteamP2PClient();
			((SteamP2PClient)client).Connect(selectedLobby);

			Connected(client);
		}

		public void Host()
		{
			server = new SteamP2PServer(maximumNumberOfPlayers);
			((SteamP2PServer)server).Host(SteamUser.GetSteamID(), isPrivateLobby ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic, OnLobbyReady);

			server.playerTimeout += (player, sender) => { Debug.Log("Player " + player.NetworkId + " timed out"); };

			Connected(server);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.H))
				Host();
			else if (Input.GetKeyDown(KeyCode.C))
				Connect();
			else if (Input.GetKeyDown(KeyCode.L))
			{
				// Refresh lobby list
			}
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

			if (useInlineChat && networker.IsServer)
				SceneManager.sceneLoaded += CreateInlineChat;

			if (networker is IServer)
			{
				if (!DontChangeSceneOnConnect)
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
				else
					NetworkObject.Flush(networker); //Called because we are already in the correct scene!
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="mode"></param>
		private void CreateInlineChat(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= CreateInlineChat;
			var chat = NetworkManager.Instance.InstantiateChatManager();
			DontDestroyOnLoad(chat.gameObject);
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
			var personalName = SteamFriends.GetPersonaName();

			var gameName = serverName.text == "" ? $"{personalName}'s game" : serverName.text;

			var lobbyId = ((SteamP2PServer) server).LobbyID;
			SteamMatchmaking.SetLobbyData(lobbyId, "name", gameName);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameId", gameId);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameType", type);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameMode", mode);
			SteamMatchmaking.SetLobbyData(lobbyId, "fnr_gameDesc", comment);
		}
	}
}
