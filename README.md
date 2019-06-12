# Forge Networking Remastered Steamworks.NET Example

Example project demonstrating how to use the Steamworks.Net integration in Forge to host and join games.

* Open the repo in Unity
* Install Steamworks.Net for Unity (https://steamworks.github.io/installation/#unity-instructions)
* Make sure to include the steam_appid.txt file with your games executable.
* Make sure an instance of steam is running in the background.
* Test your application with a built executable exclusively. If you don't, the Steam API will flag the unity editor as the game process, and it will always be listed as "Running" in your steam library, until you restart Unity. If you don't do this, your steam profile will never exit lobbies, and you'll end up with a bunch of "ghost lobbies"

## Notes
* There have been a few bug fixes that are not yet on the master/asset store version of FNR so please use a recent dev nightly or the develop branch code directly if you want to only to implement the the join/host code in your project. 
* Currently there is a bug in the SteamP2PServer code where the lobby max member count is hard coded to be `5`.
[Until a fix is in place](https://github.com/BeardedManStudios/ForgeNetworkingRemastered/pull/284) please change line 186 of the `SteamP2PServer.cs` to read
  `m_CreateLobbyResult = SteamMatchmaking.CreateLobby(lobbyType, MaxConnections);` 
