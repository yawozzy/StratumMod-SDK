using Stratum;

namespace ExampleMod;

// Hotkeys are listed on the mod's page; players rebind them there and STRATUM saves their choice.
sealed class HotkeyExamples
{
    readonly StratumMod mod;
    int presses;
    float held;

    public HotkeyExamples(StratumMod mod) => this.mod = mod;

    public void Register()
    {
        // Press: once per press. Ctrl + H by default.
        mod.Input.Bind("hello", "Say hello", () => { presses++; mod.Log.Info("Hello from a hotkey"); }, Key.LeftCtrl, Key.H);
        // Hold: every frame while the keys are down.
        mod.Input.Bind("hold", "Hold to count time", () => { }, HotkeyMode.Hold, Key.J);
        // Release: once when the keys go up.
        mod.Input.Bind("release", "Log on release", () => mod.Log.Info("Released"), HotkeyMode.Release, Key.K);
    }

    public void Update(float deltaTime)
    {
        if (mod.Input.IsHeld("hold")) held += deltaTime;
    }

    public void Draw()
    {
        mod.UI.Label("Hotkeys of this mod are listed below the main page; click one to rebind it.");
        mod.UI.Label("\"" + mod.Input.Describe("hello") + "\" pressed " + presses + " time(s).");
        mod.UI.Label("\"" + mod.Input.Describe("hold") + "\" held for " + held.ToString("0.0") + " s.");
        mod.UI.Hint("Hotkeys pause while the STRATUM menu is open.");
    }
}
