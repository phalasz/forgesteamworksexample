using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForgeSteamworksNETExample
{
	public class JoinMenu : MonoBehaviour
	{
		/// <summary>
		/// The server list UI container element
		/// </summary>
		public ScrollRect servers;

		/// <summary>
		/// The template UI element representing an entry in the server list
		/// </summary>
		public ServerListEntry serverListEntryTemplate;

		/// <summary>
		/// The container (parent) for the server list items
		/// </summary>
		public RectTransform serverListContentRect;

		/// <summary>
		/// Reference to the connect button
		/// </summary>
		public Button connectButton;

		/// <summary>
		/// Reference to the connect button's label element
		/// </summary>
		public Text connectButtonLabel;

		// TODO: could be exposed on UI to only list games played by steam friends
		private bool onlyShowFriendsGames;

		/// <summary>
		/// Internal index to track which list item has been clicked on
		/// </summary>
		private int selectedServer = -1;

		/// <summary>
		/// The list of servers the client knows about
		/// </summary>
		private List<ServerListItemData> serverList = new List<ServerListItemData>();

		/// <summary>
		/// The height of one server list entry used for repositioning the list items
		/// </summary>
		private float serverListEntryTemplateHeight;

		/// <summary>
		/// Timer to track when the server list should be re-requested
		/// </summary>
		private float nextListUpdateTime = 0f;

		/// <summary>
		/// Reference to the multiplayer menu
		/// </summary>
		private SteamworksMultiplayerMenu mpMenu;

		// Steamworks API callback methods
		private Callback<LobbyMatchList_t> callbackLobbyListRequest;
		private Callback<LobbyDataUpdate_t> callbackLobbyDataUpdate;

		private void Awake()
		{
			// Init the MainThreadManager if it has not been already
			MainThreadManager.Create();

			// Store a reference to the Multiplayer menu
			mpMenu = this.GetComponentInParent<SteamworksMultiplayerMenu>();
			serverListEntryTemplateHeight = ((RectTransform) serverListEntryTemplate.transform).rect.height;

			// Disable the connect button until the user has selected a server
			connectButton.interactable = false;

			// Make sure steam callbacks are set
			callbackLobbyListRequest = Callback<LobbyMatchList_t>.Create(OnLobbyListRequested);
			callbackLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);

			// Request the initial lobby list
			GetAvailableLobbyList();
		}

		private void Update()
		{
			if (Time.time > nextListUpdateTime)
			{
				// Refresh lobbies from steam
				GetAvailableLobbyList();

				// TODO:
				// Is re-requesting with the same filters enough and simply not add the server if it is already in the list?
				// What should happen to servers that are not returned in the new request? Should they be removed?

				// TODO: Might worth extracting the 20.0f into a const or a field to be configured via the inspector
				//       Is refreshing the list every 20ish seconds good enough?
				nextListUpdateTime = Time.time + 20.0f + UnityEngine.Random.Range(0.0f, 1.0f);
			}

			foreach (var server in serverList)
			{
				if (Time.time > server.NextUpdate)
				{
					// Time to re-request the server information
					SteamMatchmaking.RequestLobbyData(server.SteamId);

					// TODO: Might worth extracting the 5.0f into a const or a field to be configured via the inspector
					//       Is re-requesting the server/lobby info every 5 seconds good enough? Should it be less frequent?
					//
					server.NextUpdate = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);
				}
			}
		}

		/// <summary>
		/// Called when a server list item is clicked. It will automatically connect on double click.
		/// </summary>
		/// <param name="e"></param>
		public void OnServerItemPointerClick(BaseEventData e)
		{
			// Ignore clicks if Forge is in the process of connecting.
			if (mpMenu.IsConnecting)
				return;

			var eventData = (PointerEventData)e;
			for (int i = 0; i < serverList.Count; ++i) {
				if (serverList[i].ListItem.gameObject != eventData.pointerPress) continue;

				SetSelectedServer(i);
				if (eventData.clickCount == 2)
					mpMenu.Connect();

				return;
			}
		}

		/// <summary>
		/// Add a server to the list of servers
		/// </summary>
		/// <param name="steamId">The <see cref="CSteamID"/> of the lobby/server to add to the list</param>
		private void AddServer(CSteamID steamId)
		{
			for (int i = 0; i < serverList.Count; ++i)
			{
				var server = serverList[i];
				if (server.SteamId == steamId)
				{
					// Already have that server listed nothing else to do
					return;
				}
			}

			var serverListItemData = new ServerListItemData {
				ListItem = GameObject.Instantiate<ServerListEntry>(serverListEntryTemplate, servers.content),
				SteamId = steamId
			};

			// Make the list item visible
			serverListItemData.ListItem.gameObject.SetActive(true);

			// Make sure we periodically re-request the lobby/server data so the server information in the list is
			// up-to-date
			serverListItemData.NextUpdate = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);

			// Add the server to the list.
			serverList.Add(serverListItemData);
			// Make sure the the newly added server is not selected on the UI
			SetListItemSelected(serverListItemData, false);

			// Make sure every item in the UI is positioned well
			RepositionItems();
		}

		/// <summary>
		/// Remove a server from the list
		/// </summary>
		/// <param name="index">The index of the server in the list</param>
		private void RemoveServer(int index)
		{
			var o = serverList[index];
			RemoveServer(o);
		}

		/// <summary>
		/// Remove a server from the server list based on the list data
		/// </summary>
		/// <param name="item">The <see cref="ServerListItemData"/> to be removed</param>
		private void RemoveServer(ServerListItemData item)
		{
			Destroy(item.ListItem.gameObject);
			serverList.Remove(item);
			RepositionItems();
		}

		/// <summary>
		/// Reposition the server list items after a add/remove operation
		/// </summary>
		private void RepositionItems()
		{
			for (int i = 0; i < serverList.Count; i++) {
				PositionItem(serverList[i].ListItem.gameObject, i);
			}

			var sizeDelta = serverListContentRect.sizeDelta;
			sizeDelta.y = serverList.Count * serverListEntryTemplateHeight;
			serverListContentRect.sizeDelta = sizeDelta;
		}

		/// <summary>
		/// Set the position of an item in the server list
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		private void PositionItem(GameObject item, int index)
		{
			var rectTransform = (RectTransform)item.transform;
			rectTransform.localPosition = new Vector3(0.0f, -serverListEntryTemplateHeight * index, 0.0f);
		}

		/// <summary>
		/// Select a server in the list and prefill the steam lobby id hidden field
		/// </summary>
		/// <param name="index"></param>
		private void SetSelectedServer(int index)
		{
			// If we selected the server that was already selected then nothing else to do.
			if (selectedServer == index)
				return;

			selectedServer = index;

			// Set the border for the selected list item
			for (int i = 0; i < serverList.Count; i++) {
				SetListItemSelected(serverList[i], index == i);
			}

			if (index >= 0) {
				// We have selected a server from the server list so enable the connect button
				connectButton.interactable = true;
				// Rename the connect button to state the name of the server/lobby to be joined
				connectButtonLabel.text = $"Connect to {serverList[index].ListItem.serverName.text}";
				// Tell the multiplayer menu the steam id of the lobby that was selected
				mpMenu.SetSelectedLobby(serverList[selectedServer].SteamId);
			}
			else
			{
				connectButton.interactable = false;
				connectButtonLabel.text = "Connect";
				mpMenu.SetSelectedLobby(CSteamID.Nil);
			}
		}

		/// <summary>
		/// Set the border around the selected server entry
		/// </summary>
		/// <param name="data"></param>
		/// <param name="selected"></param>
		private void SetListItemSelected(ServerListItemData data, bool selected)
		{
			data.ListItem.GetComponent<Image>().enabled = selected;
		}

		/// <summary>
		/// Get all available lobbies for this game.
		/// </summary>
		private void GetAvailableLobbyList()
		{
			if (SteamManager.Initialized)
			{
				if (!onlyShowFriendsGames)
				{
					// Get servers from everywhere change it to ELobbyDistanceFilter.Default to get only near servers
					SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
					// Only get games that have our id
					SteamMatchmaking.AddRequestLobbyListStringFilter("fnr_gameId", mpMenu.gameId,
						ELobbyComparison.k_ELobbyComparisonEqual);
					// Uncomment this if the default count of 50 is not enough.
					//SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);

					// Request list of lobbies based on above filters from Steam
					SteamMatchmaking.RequestLobbyList();
				}
				else
					GetFriendGamesList();
			}
		}

		/// <summary>
		/// Check if any of the current user's friends play this game and add the lobby to the server list if they do.
		/// </summary>
		private void GetFriendGamesList()
		{
			// Get the number of regular friends of the current local user
			var friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			if (friendCount == -1)
				return;

			for (int i = 0; i < friendCount; ++i)
			{
				// Get the Steam ID of the friend
				var friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

				// Get what game the friend is playing
				FriendGameInfo_t gameInfo;
				if (SteamFriends.GetFriendGamePlayed(friendSteamId, out gameInfo))
				{
					// If they are playing this game as well then get their lobby id
					if (gameInfo.m_gameID.AppID() == SteamUtils.GetAppID())
					{
						AddServer(gameInfo.m_steamIDLobby);
					}
				}
			}
		}

		/// <summary>
		/// Handle the RequestLobbyList Steam API callback
		/// </summary>
		/// <param name="result">The <see cref="LobbyMatchList_t"/> result set</param>
		private void OnLobbyListRequested(LobbyMatchList_t result)
		{
			for (int i = 0; i < result.m_nLobbiesMatching; i++)
			{
				var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
				AddServer(lobbyId);
				SteamMatchmaking.RequestLobbyData(lobbyId);
			}
		}

		/// <summary>
		/// Handle the RequestLobbyData Steam API callback
		/// </summary>
		/// <param name="result">The <see cref="LobbyDataUpdate_t"/> result set</param>
		private void OnLobbyDataUpdated(LobbyDataUpdate_t result)
		{
			for (int i = 0; i < serverList.Count; i++)
			{
				if (serverList[i].SteamId.m_SteamID == result.m_ulSteamIDLobby)
				{
					// Lobby no longer exists so remove it from the list
					if (result.m_bSuccess == 0)
					{
						RemoveServer(i);
						return;
					}

					serverList[i].ListItem.serverName.text = SteamMatchmaking.GetLobbyData(serverList[i].SteamId, "name");
					serverList[i].ListItem.gameType.text = SteamMatchmaking.GetLobbyData(serverList[i].SteamId, "fnr_gameType");
					serverList[i].ListItem.gameMode.text = SteamMatchmaking.GetLobbyData(serverList[i].SteamId, "fnr_gameMode");
					var maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(serverList[i].SteamId);
					var currPlayers = SteamMatchmaking.GetNumLobbyMembers(serverList[i].SteamId);
					serverList[i].ListItem.playerCount.text = $"{currPlayers}/{maxPlayers}";
					return;
				}
			}
		}
	}

	/// <summary>
	/// Internal class storing server data to be displayed in the server list
	/// </summary>
	internal class ServerListItemData
	{
		/// <summary>
		/// The <see cref="CSteamID"/> of the lobby/server
		/// </summary>
		public CSteamID SteamId;

		/// <summary>
		/// Reference to the UI element of this server
		/// </summary>
		public ServerListEntry ListItem;

		/// <summary>
		/// Time of next server information upate
		/// </summary>
		public float NextUpdate;
	}
}
