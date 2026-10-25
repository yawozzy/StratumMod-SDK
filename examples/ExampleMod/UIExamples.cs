using Stratum;

namespace ExampleMod;

// Pages opened with UI.PageButton. Each method describes its page every frame while it is open.
sealed class UIExamples
{
    readonly StratumMod mod;
    readonly Settings settings;
    bool toggle = true;
    float decimalValue = 0.5f;
    int wholeValue = 3;
    string text = "Hello";
    string digits = "";
    int choice;
    float progress;
    readonly string[] choices = { "Low", "Medium", "High" };

    public UIExamples(StratumMod mod, Settings settings)
    {
        this.mod = mod;
        this.settings = settings;
    }

    ModUI UI => mod.UI;

    public void DrawElements()
    {
        UI.Header("Buttons");
        using (UI.Row())
        {
            if (UI.Button("Button", tooltip: "A plain button")) mod.Log.Info("Button clicked");
            if (UI.Button("Main action", primary: true)) mod.Log.Info("Main action clicked");
        }

        UI.Header("Switch");
        if (UI.Toggle("Example switch", ref toggle, "Returns true when it changes")) mod.Log.Info("Switch: " + toggle);

        UI.Header("Sliders");
        UI.Slider("Decimal value", ref decimalValue, 0, 1, step: 0.05f);
        UI.Slider("Whole value", ref wholeValue, 0, 10);

        UI.Header("Text fields");
        if (UI.Input("Any text", ref text, maxLength: 32, hint: "Type here")) mod.Log.Info("Text: " + text);
        // Validate input yourself: here only digits are kept.
        if (UI.Input("Digits only", ref digits, maxLength: 8)) digits = new string(digits.Where(char.IsDigit).ToArray());

        UI.Header("Drop-down list");
        if (UI.Dropdown("Quality", ref choice, choices)) mod.Log.Info("Quality: " + choices[choice]);

        UI.Header("Progress");
        progress = (progress + 0.01f) % 1f;
        UI.Progress("Loading example", progress);

        UI.Header("Text");
        UI.Label("A label wraps to the page width when the text is long enough to need more than one line.");
        UI.Hint("A hint is muted text for explanations.");
        UI.Link("Open a web page", "https://github.com/");
    }

    public void DrawLayout()
    {
        UI.Label("Widgets go top to bottom. A Row puts them side by side; a Group frames them as a card.");

        using (UI.Group("A group"))
        {
            UI.Label("Widgets inside a group.");
            using (UI.Row())
            {
                UI.Button("Left");
                UI.Button("Middle");
                UI.Button("Right");
            }
        }

        UI.Space(20);
        UI.Separator();

        using (UI.Row())
        {
            using (UI.Group("Left card")) UI.Label("Groups in a row sit next to each other.");
            using (UI.Group("Right card")) UI.Toggle("A switch in a card", ref toggle);
        }

        UI.PageButton("A deeper page", DrawDeeper, "Nested page");
    }

    void DrawDeeper()
    {
        UI.Label("Pages can be nested. The menu shows Back automatically.");
        if (UI.Button("Back from code")) UI.Back();
    }

    public void DrawSettings()
    {
        UI.Hint("These values are saved to STRATUM/Configs/stratum.example/settings.json.");
        bool changed = false;
        changed |= UI.Toggle("Show greeting on load", ref settings.Data.Greeting);
        changed |= UI.Slider("Volume", ref settings.Data.Volume, 0, 100, step: 1);
        changed |= UI.Input("Nickname", ref settings.Data.Nickname, 24);
        if (changed) settings.Save(mod.Log);
    }
}
