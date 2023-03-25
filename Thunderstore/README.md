# VikingSails

<p align="center">
  <img src="https://i.imgur.com/MLdkF7r.png" alt="image" width="523" height="440">
</p>

**This mod was requested by ALo#8803 and ErDu#3488**

VikingSails is a mod that allows you to change the sails of ships. You can use URLs to set the sails, or if the server forces the URL it will use that one.

**Version checks with itself. If installed on the server, it will kick clients who do not have it installed.**

**This mod uses ServerSync, if installed on the server and all clients, it will sync all configs with [Synced with Server] tags to client**

**This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients.**

`Note: This mod can be installed client only, but will not sync the configuration between clients. Clients can still see other's sails this way. It's only needed on the server if you wish to enforce any of the configuration values that have [Synced with Server] tags.`

<details>
<summary><b>Configuration Options</b></summary>

### General

> Configuration File Name: `Azumatt.VikingSails.cfg`

Lock Configuration [Synced with Server]
* If on, the configuration is locked and can be changed by server admins only.
    * Default Value: On

Use Server Sail URL [Synced with Server]
* A toggle that when turned on, sets all ship sails to use the Server Sail URL.
    * Default Value: On

Server Sail URL [Synced with Server]
* Put a valid image URL here, this entry field should contain the URL of the image to use for the sails when the `Use Server Sail URL` toggle is on.
    * Default Value: https://i.imgur.com/TbcJ3LU.png

Edit Key [Not Synced with Server]
* A keyboard shortcut that allows the player to interact with the ship to change the sail image
    * Default Value: Mouse1

Require Key Press [Synced with Server]
* A toggle that when turned on, requires the player to hold down the `Edit Key` in order to interact with the ship to change the sails.
    * Default Value: On

    To interact with a ship and change its sails, look at the ship and hold down the `Edit Key` (default: right mouse button). If `Require Key Press` is turned off, you can simply interact with the ship to change the sails via the prompt URL window. If `Require Key Press` is turned on, you must hold down the `Edit Key` and then interact.

    If `Use Server Sail URL` is turned on, the ship's sails will be updated with the URL set in the `Server Sail URL` field. If `Use Server Sail URL` is turned off, you can set the URL for the sails by interacting with the ship. This will open a text entry field where you can paste the URL for the image you want to use.
</details>

<details>
<summary><b>Installation Instructions</b></summary>

### Manual Installation

`Note: (Manual installation is likely how you have to do this on a server, make sure BepInEx is installed on the server correctly)`

1. **Download the latest release of BepInEx.**
2. **Extract the contents of the zip file to your game's root folder.**
3. **Download the latest release of VikingSails from Thunderstore.io.**
4. **Extract the contents of the zip file to the `BepInEx/plugins` folder.**
5. **Launch the game.**

### Installation through r2modman or Thunderstore Mod Manager

1. **Install [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/) or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager).**

   > For r2modman, you can also install it through the Thunderstore site.
   ![](https://i.imgur.com/s4X4rEs.png "r2modman Download")
    
    > For Thunderstore Mod Manager, you can also install it through the Overwolf app store
    ![](https://i.imgur.com/HQLZFp4.png "Thunderstore Mod Manager Download") 
2. **Open the Mod Manager and search for "VikingSails" under the Online tab. `Note: You can also search for "Azumatt" to find all my mods.`**


   ![](https://i.imgur.com/5CR5XKu.png) 
3. **Click the Download button to install the mod.**
4. **Launch the game.**

</details>


<details>
<summary><b>How To Translate This Mod</b></summary>

To add additional localizations to VikingSails, your users can create a file with the name VikingSails.Language.yml or VikingSails.Language.json anywhere inside of the Bepinex folder. For example, to add a French translation to VikingSails, a user could create a VikingSails.French.yml file inside of the config folder and add French translations there.

The format of the file is as follows, this is the current English translation file embedded in the mod:

```yaml
set_url: "Set URL"
sail_url: "Sail URL"
server_sail_url_deny: "Server is in control of the URL, denied"
```


</details>

---

**Feel free to reach out to me on discord if you need manual download assistance.**


# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>