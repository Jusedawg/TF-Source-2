<p align="center">
	<img src="https://github.com/AmperSoftware/TF-Source-2/blob/dev/ui/Menu/tfs2_logotype.png">
</p>
<hr>

[Team Fortress: Source 2](https://tfsource2.com/about) (TFS2) is a project powered by s&box made by Facepunch, the spiritual successor to Garry's Mod. It aims to recreate the experience and mechanics of TF2 on Source 2, taking advantage of the updated engine's graphics and capabilities. The project has been internally developed by Amper Software and it has been made open source for everyone to access, players and developers alike.

# Installing

This part aims to guide developers with installing and setting up the addon for local development. For general users and players it is recommended to use s&box's in-game addon browser to play the gamemode.

- Navigate to your s&box installation directory. For example, on Windows this would be the ``Steam/steamapps/common/sbox/`` folder.
- Go into the ``addons`` folder and clone the repository.
- Initialize submodules. We use our [s&box FPS SDK](https://github.com/AmperSoftware/sbox-FPS-SDK) as the core of the gamemode. If you have cloned the repository using a GUI client such as GitHub Desktop the submodule should have been initialized automatically. If not, navigate to the repository folder and run: ``git submodule update --init --recursive``. You can then navigate to the ``code/Libraries/FPS`` folder and verify that it contains content.
- Run the developer version of s&box (sbox-dev.exe). Once loaded, add the addon using the Project context menu, near the top (Project -> Add Existing From Disk) by selecting the ``.addon`` file and verify that it is active and enabled by hovering over the checkmark icon.

Congrats! Your local version of TFS2 is ready to be played and modified.  
If you encounter any issues during installation or the overall setup process you can contact us on our [Discord](https://discord.gg/tMnTsUsVjP).

# Launching & Updating
An easy way to launch the game mode is by using [launch configs](https://media.discordapp.net/attachments/712252851283296260/1043964479613976727/image.png).  
You can also launch the gamemode by either using the in-game gamemode browser or by using the console. If you want to use the console, run the following commands:
```
gamemode local.tfs2
map mapname
```

In order to update the gamemode all you have to do is just git pull the new changes from the remote repository. Most of the time, the game will succesfully recognize the new changes and hotload them. However if you are doing some major git operations (like checking out another branch or merging it), it is recommended to close the game before you perform them.

Also, we gitignore most ``*_c`` files so you will need to wait for the gamemode to compile the first time you launch it.

# Contributing

Interested in fixing a bug or implementing a feature? Whether it's a major change or a small hotfix, you are welcome to submit it through a pull request! But before that happens you must follow:
- [The Pull Request Guidelines](https://github.com/AmperSoftware/TF-Source-2/blob/dev/.github/CONTRIBUTING.md)
- [The Pull Request Template](https://github.com/AmperSoftware/TF-Source-2/blob/dev/.github/PULL_REQUEST_TEMPLATE.md)

As long as you abide by these guidelines, you can submit a pull request and an Amper developer will try to review it as soon as possible.  
Also, feel free to join our [Discord](https://discord.gg/tMnTsUsVjP) and chat with other developers about your ideas in the special channels.

**For localization contributions check out [this repository.](https://github.com/AmperSoftware/TF-Source-2-Localization)**
# License & Disclaimer

This repository is licensed under the [MIT license](https://github.com/AmperSoftware/TF-Source-2/blob/dev/LICENSE.md).

However, some assets included such as models, sounds, animations, shaders and more were originally created by Valve Corporation and therefore are under Valve's intellectual property.

**[Third Party Licenses](https://github.com/AmperSoftware/TF-Source-2/blob/dev/thirdpartylicenses.md)**
