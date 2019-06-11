# Forge Networking Remastered Steamworks.NET Example

Example project demonstrating how to use the Steamworks.Net integration in Forge to host and join games.


* Install Steamworks.Net for Unity (https://steamworks.github.io/installation/#unity-instructions)
* Make sure to include the steam_appid.txt file with your games executable.
* Make sure an instance of steam is running in the background.
* Test your application with a built executable exclusively. If you don't, the Steam API will flag the unity editor as the game process, and it will always be listed as "Running" in your steam library, until you restart Unity. If you don't do this, your steam profile will never exit lobbies, and you'll end up with a bunch of "ghost lobbies"

