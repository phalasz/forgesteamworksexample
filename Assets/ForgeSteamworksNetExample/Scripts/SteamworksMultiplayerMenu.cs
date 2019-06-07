using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Lobby;
using BeardedManStudios.SimpleJSON;
using System.Collections.Generic;
using Steamworks;
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

		[SerializeField]
		private SteamAvatar playerAvatar;


		private NetworkManager mgr = null;
		private NetWorker server;

		private bool isPrivateLobby;

		private List<Button> _uiButtons = new List<Button>();
		private bool _matchmaking = false;

		private Callback<LobbyCreated_t> callbackLobbyCreated;

		private void Start()
		{
#if !STEAMWORKS
			Debug.LogError("Missing STEAMWORKS define. This menu will not work without it");
			throw new SystemException("Missing STEAMWORKS define. This menu will not work without it");
#endif
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

			callbackLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		}

		public void Connect()
		{
			if (connectUsingMatchmaking)
			{
				// Add custom matchmaking logic here.
				// eg.: pick a random lobby from the list of lobbies for the game
				return;
			}



			NetWorker client;


//				client = new UDPClient();
//				if (natServerHost.Trim().Length == 0)
//					((UDPClient) client).Connect(ipAddress.text, (ushort) port);
//				else
//					((UDPClient) client).Connect(ipAddress.text, (ushort) port, natServerHost, natServerPort);


//			Connected(client);
		}

		public void Host()
		{
			var myLobby = new SteamP2PServer(maximumNumberOfPlayers);
			myLobby.Host(SteamUser.GetSteamID(), isPrivateLobby ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic);

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
		/// Callback to handle the Steamworks API response on lobby creation
		/// </summary>
		/// <param name="result"></param>
		private void OnLobbyCreated(LobbyCreated_t result)
		{
			if (result.m_eResult == EResult.k_EResultOK)
			{
				var personalName = SteamFriends.GetPersonaName();
				SteamMatchmaking.SetLobbyData((CSteamID) result.m_ulSteamIDLobby, "name", $"{personalName}'s game");
			}
		}
	}
}
