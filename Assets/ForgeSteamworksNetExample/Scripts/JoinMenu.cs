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
		public ScrollRect servers;
		public ServerListEntry serverListEntryTemplate;
		public RectTransform serverListContentRect;
		public Button connectButton;
		public Text connectButtonLabel;

		// TODO: could be exposed on UI to only list games played by steam friends
		private bool onlyShowFriendsGames;

		private int selectedServer = -1;
		private List<ServerListItemData> serverList = new List<ServerListItemData>();
		private float serverListEntryTemplateHeight;
		private float nextListUpdateTime = 0f;
		private SteamworksMultiplayerMenu mpMenu;

		private Callback<LobbyMatchList_t> callbackLobbyListRequest;
		private Callback<LobbyDataUpdate_t> callbackLobbyDataUpdate;

		private void Awake()
		{
			// Init the MainThreadManager
			MainThreadManager.Create();

			mpMenu = this.GetComponentInParent<SteamworksMultiplayerMenu>();
			serverListEntryTemplateHeight = ((RectTransform) serverListEntryTemplate.transform).rect.height;

			connectButton.interactable = false;

			callbackLobbyListRequest = Callback<LobbyMatchList_t>.Create(OnLobbyListRequested);
			callbackLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);

			GetAvailableLobbyList();
		}

		private void Update()
		{
			if (Time.time > nextListUpdateTime)
			{
				// Refresh lobbies from steam

				nextListUpdateTime = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);
			}
		}

		/// <summary>
		/// Called when a server list item is clicked. It will automatically connect on double click.
		/// </summary>
		/// <param name="e"></param>
		public void OnServerItemPointerClick(BaseEventData e)
		{
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
		/// <param name="steamId"></param>
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

			var dataCount = SteamMatchmaking.GetLobbyDataCount(steamId);
			for (int i = 0; i < dataCount; i++)
			{
				string key;
				string value;
				if (SteamMatchmaking.GetLobbyDataByIndex(steamId, i, out key, 255, out value, 8192))
				{
					Debug.Log($"{steamId}: {key}={value}");
				}
			}

			var serverListItemData = new ServerListItemData {
				ListItem = GameObject.Instantiate<ServerListEntry>(serverListEntryTemplate, servers.content),
				SteamId = steamId
			};
			serverListItemData.ListItem.gameObject.SetActive(true);

			UpdateItem(serverListItemData);
			//serverListItemData.NextUpdate = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);

			serverList.Add(serverListItemData);
			SetListItemSelected(serverListItemData, false);

			RepositionItems();
		}

		/// <summary>
		/// Remove a server from the list
		/// </summary>
		/// <param name="index"></param>
		private void RemoveServer(int index)
		{
			var o = serverList[index];
			RemoveServer(o);
		}

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
			if (selectedServer == index)
				return;

			selectedServer = index;

			for (int i = 0; i < serverList.Count; i++) {
				SetListItemSelected(serverList[i], index == i);
			}

			if (index >= 0) {
				// TODO: Enable connect button
				connectButton.interactable = true;
				// TODO: set selected lobby id
				connectButtonLabel.text = $"(c) Connect to {serverList[index].ListItem.serverName.text}";
				mpMenu.SetSelectedLobby(serverList[selectedServer].SteamId);
			}
			else
			{
				// TODO: Disable connect button
				connectButton.interactable = false;
				// TODO: reset selected lobby id
				connectButtonLabel.text = "(c) Connect";
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
		/// Update a specific server's details on the server list.
		/// </summary>
		/// <param name="option">The server display information to update</param>
		private void UpdateItem(ServerListItemData option)
		{
//			option.ListItem.hostName.text = option.Hostname;
//
//			if (option.SqpQuery.ValidResult)
//			{
//				var sid = option.SqpQuery.ServerInfo.ServerInfoData;
//				option.ListItem.serverName.text = $"{sid.ServerName} ({option.LocalOrGlobal})";
//				option.ListItem.playerCount.text = $"{sid.CurrentPlayers.ToString()}/{sid.MaxPlayers.ToString()}";
//				option.ListItem.pingTime.text = $"{option.SqpQuery.RTT.ToString()} ms";
//			}
//			else
//			{
//				option.ListItem.serverName.text = "Server offline";
//				option.ListItem.playerCount.text = "-/-";
//				option.ListItem.pingTime.text = "--";
//			}
		}

		private void GetAvailableLobbyList()
		{
			if (SteamManager.Initialized)
			{
				if (!onlyShowFriendsGames)
				{
					SteamMatchmaking.AddRequestLobbyListStringFilter("fnr_gameId", mpMenu.gameId,
						ELobbyComparison.k_ELobbyComparisonEqual);
					SteamMatchmaking.RequestLobbyList();
				}
				else
					GetFriendGamesList();
			}
		}

		private void GetFriendGamesList()
		{
			var friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			if (friendCount == -1)
				return;

			for (int i = 0; i < friendCount; ++i)
			{
				var friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
				FriendGameInfo_t gameInfo;
				if (SteamFriends.GetFriendGamePlayed(friendSteamId, out gameInfo))
				{
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

	internal class ServerListItemData
	{
		public CSteamID SteamId;
		public ServerListEntry ListItem;
	}
}
