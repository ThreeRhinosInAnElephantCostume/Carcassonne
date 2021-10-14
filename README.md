# Carcassonne

This is a heavily WIP project. Nothing much to see here.


## Setting up the development environment

### Requirements:

1. [Godot 3.3.4 or newer](https://godotengine.org/download/) (4.x is not supported) - Mono version.
2. mono and dotnet-sdk. [See the relevant Godot documentation](https://docs.godotengine.org/en/stable/getting_started/scripting/c_sharp/c_sharp_basics.html).
3. [dotnet-format](https://www.nuget.org/packages/dotnet-format/).
4. [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json).
5. (vscode) [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp), [godot-tools](https://marketplace.visualstudio.com/items?itemName=geequlim.godot-tools), [C# tools for godot](https://marketplace.visualstudio.com/items?itemName=neikeq.godot-csharp-vscode), as well as [gdscript](https://marketplace.visualstudio.com/items?itemName=jjkim.gdscript) extensions.


See Scripts/vscode for example launch.json and settings.json files.
See Scripts/githooks for the pre-commit hook required for automatic formating.

Additionally, a setup script (setup.sh) has been provided, nominally capable of installing both mono and dotnet dependencies, as well as setting up the git hook.
