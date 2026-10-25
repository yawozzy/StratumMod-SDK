using Stratum;

namespace ExampleMod;

// Entry class: derive from StratumMod and describe the mod with [StratumMod].
// STRATUM creates one instance and calls the overrides on the Unity main thread.
[StratumMod("stratum.example", "Example Mod", "1.0.0", Author = "STRATUM", Description = "Shows every part of Stratum.API: menu UI, hotkeys, sync and game access.")]
public sealed class Example : StratumMod
{
    readonly Settings settings = new();
    UIExamples ui = null!;
    HotkeyExamples hotkeys = null!;
    SyncExamples sync = null!;
    GameExamples game = null!;

    public override void OnLoad()
    {
        Log.Info("Example Mod loaded");

        settings.Load(ConfigDirectory, Log);
        ui = new UIExamples(this, settings);
        hotkeys = new HotkeyExamples(this);
        sync = new SyncExamples(this);
        game = new GameExamples(this);

        hotkeys.Register();
        sync.Register();
    }

    public override void OnUpdate(float deltaTime)
    {
        hotkeys.Update(deltaTime);
    }

    // The mod's page in the STRATUM menu: Insert → My mods → Example Mod → «Настройки мода».
    public override void OnDrawUI()
    {
        UI.Header("Example Mod");
        UI.Hint("Every section below is a separate example. Open one to see how it is built.");

        using (UI.Group("Examples"))
        {
            UI.PageButton("UI elements", ui.DrawElements, tooltip: "Buttons, switches, sliders, fields, lists");
            UI.PageButton("Layout", ui.DrawLayout, tooltip: "Rows, groups, spacing");
            UI.PageButton("Saved settings", ui.DrawSettings, tooltip: "Values kept between game sessions");
            UI.PageButton("Hotkeys", hotkeys.Draw);
            UI.PageButton("Multiplayer sync", sync.Draw);
            UI.PageButton("Game access", game.Draw);
        }

        // Put your mod's page here (only http/https links are opened).
        UI.Link("Mod page", "https://github.com/");
    }

    public override void OnQuit() => settings.Save(Log);

    // Logo for the STRATUM menu, embedded in the DLL (see ExampleMod.csproj).
    public override byte[]? GetIcon() => LoadEmbeddedResource("ExampleMod.icon.png");
}
