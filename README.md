# Forge Networking Remastered Steamworks.NET Example

Example project demonstrating how to use the Steamworks.Net integration in Forge to host and join games.

* Make sure you have [large file support](https://git-lfs.github.com/) installed for git
* Clone the repository. If you download as a zip then all images will be broken.
* Open the repo in Unity (Project was made with Unity 2018.2.15f)
* Make sure that you have `STEAMWORKS` set in the [Scripting Define Symbols list in your Player settings](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)
* Install Steamworks.Net for Unity (https://steamworks.github.io/installation/#unity-instructions)
* Make sure to include the steam_appid.txt file with your games executable.
* Make sure an instance of steam is running in the background.
* Test your application with a built executable exclusively. If you don't, the Steam API will flag the unity editor as the game process, and it will always be listed as "Running" in your steam library, until you restart Unity. If you don't do this, your steam profile will never exit lobbies, and you'll end up with a bunch of "ghost lobbies"

## Notes
* If you use a different version of Unity to the one the project was made with (2018.2.15f) then please make sure to upgrade the installed packages via the package manager.
* There have been a few bug fixes that are not yet on the master/asset store version of FNR so please use a recent dev nightly or the develop branch code directly if you want to only to implement the the join/host code in your project. This project already has those fixes.
* Currently there is a bug in the SteamP2PServer code where the lobby max member count is hard coded to be `5`.
[Until a fix is in place](https://github.com/BeardedManStudios/ForgeNetworkingRemastered/pull/284) please change line 186 of the `SteamP2PServer.cs` to read
  `m_CreateLobbyResult = SteamMatchmaking.CreateLobby(lobbyType, MaxConnections);` 
