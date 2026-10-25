using System.Text.Json;
using Stratum;

namespace ExampleMod;

// Settings kept between sessions as JSON in the mod's own folder (StratumMod.ConfigDirectory).
sealed class Settings
{
    public sealed class Values
    {
        // Fields (not properties) so the UI can change them with ref; IncludeFields below saves them.
        public bool Greeting = true;
        public float Volume = 50;
        public string Nickname = "";
    }

    static readonly JsonSerializerOptions Json = new() { IncludeFields = true, WriteIndented = true };
    string path = "";

    public Values Data { get; private set; } = new();

    public void Load(string folder, ModLog log)
    {
        path = Path.Combine(folder, "settings.json");
        try
        {
            if (File.Exists(path)) Data = JsonSerializer.Deserialize<Values>(File.ReadAllText(path), Json) ?? new Values();
        }
        catch (Exception error) { log.Warning("Settings were reset: " + error.Message); }
        if (Data.Greeting) log.Info("Hello, " + (Data.Nickname.Length > 0 ? Data.Nickname : "driver") + "!");
    }

    public void Save(ModLog log)
    {
        try { File.WriteAllText(path, JsonSerializer.Serialize(Data, Json)); }
        catch (Exception error) { log.Error("Cannot save settings", error); }
    }
}
