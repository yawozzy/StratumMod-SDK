using Stratum;

namespace MyMod;

// 1. Change the id (unique, lowercase), the name and the author.
[StratumMod("yourname.mymod", "My Mod", "1.0.0", Author = "Your name", Description = "What the mod does, in one line.")]
public sealed class MyMod : StratumMod
{
    bool enabled = true;

    // 2. Runs once when the game has started.
    public override void OnLoad()
    {
        Log.Info("My Mod loaded");
        Input.Bind("toggle", "Turn My Mod on/off", () => enabled = !enabled, Key.F8);
    }

    // 3. Runs every frame on the Unity main thread.
    public override void OnUpdate(float deltaTime)
    {
        if (!enabled) return;
    }

    // 4. The mod's page in the STRATUM menu.
    public override void OnDrawUI()
    {
        UI.Toggle("Enabled", ref enabled);
        UI.Hint("Hotkey: " + Input.Describe("toggle"));
    }
}
