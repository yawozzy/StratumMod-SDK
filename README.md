# Stratum SDK

SDK for C# mods for the **STRATUM** mod loader — **CarX Drift Racing Online 2**, Windows x64.
SDK для C#-модов загрузчика **STRATUM** — **CarX Drift Racing Online 2**, Windows x64.

![Example Mod in the STRATUM menu](docs/images/04-sdk-elements.png)

- **Stratum.API** — mod base class, a page in the STRATUM menu, rebindable hotkeys, multiplayer messages, direct access to Unity and CarX classes.
- **tools** — game references from your STRATUM install, one-command build, `.strmod` packer.
- **template\MyMod** — start here. **examples\ExampleMod** — every feature with comments.

## Guide / Руководство

- 🇷🇺 [docs/GUIDE-RU.md](docs/GUIDE-RU.md) — полное руководство для игроков и мододелов
- 🇬🇧 [docs/GUIDE-EN.md](docs/GUIDE-EN.md) — complete guide for players and modders

## Quick start

Needs the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and the game with STRATUM 0.12+.

```powershell
.\tools\Get-GameReferences.ps1 -Game "C:\Program Files (x86)\Steam\steamapps\common\CarX Drift Racing Online 2"
.\tools\Build-Mod.ps1 -Project .\template\MyMod
# copy template\MyMod\dist\MyMod-1.0.0.strmod to <game>\STRATUM\Mods
```

```csharp
using Stratum;

[StratumMod("yourname.mymod", "My Mod", "1.0.0", Author = "Your name")]
public sealed class MyMod : StratumMod
{
    bool enabled = true;

    public override void OnLoad() => Input.Bind("toggle", "On/off", () => enabled = !enabled, Key.F8);

    public override void OnDrawUI()
    {
        UI.Toggle("Enabled", ref enabled);
        if (UI.Button("Say hello", primary: true)) Log.Info("Hello!");
    }
}
```

Players need the [.NET 8 Runtime x64](https://dotnet.microsoft.com/download/dotnet/8.0) for C# mods.
