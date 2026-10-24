using System.Globalization;

namespace Stratum.NativeAot;

public sealed unsafe class Settings
{
    private readonly HostApi* api;
    internal Settings(HostApi* api) => this.api = api;

    public void Toggle(string key, string label, bool defaultValue, string group = "", string description = "") =>
        Define(key, "toggle", label, defaultValue ? "true" : "false", group, description);

    public void Slider(string key, string label, double defaultValue, double minimum, double maximum,
        double step = 1, string group = "", string description = "") =>
        Define(key, "slider", label, defaultValue.ToString("G17", CultureInfo.InvariantCulture),
            group, description, minimum, maximum, step);

    public void Choice(string key, string label, string defaultValue, string[] choices,
        string group = "", string description = "")
    {
        ArgumentNullException.ThrowIfNull(choices);
        if (choices.Length is < 1 or > 64 || choices.Any(x => string.IsNullOrEmpty(x) || x.Contains('|')) ||
            choices.Distinct(StringComparer.Ordinal).Count() != choices.Length)
            throw new ArgumentException("Expected 1..64 distinct choices without '|'.", nameof(choices));
        Define(key, "choice", label, defaultValue, group, description, choices: string.Join('|', choices));
    }

    public void Keybind(string key, string label, int virtualKey, string group = "", string description = "") =>
        Define(key, "keybind", label, virtualKey.ToString(CultureInfo.InvariantCulture), group, description);

    private void Define(string key, string kind, string label, string defaultValue, string group,
        string description, double minimum = 0, double maximum = 0, double step = 0, string choices = "")
    {
        using var k = new NativeText(key, 96);
        using var t = new NativeText(kind, 16);
        using var g = new NativeText(group, 128);
        using var l = new NativeText(label, 256);
        using var d = new NativeText(description, 1024);
        using var v = new NativeText(defaultValue, 1024);
        using var c = new NativeText(choices, 8192);
        var definition = new SettingDefinition
        {
            Size = (uint)sizeof(SettingDefinition), Key = k.Pointer, Kind = t.Pointer,
            Group = g.Pointer, Label = l.Pointer, Description = d.Pointer, DefaultValue = v.Pointer,
            Minimum = minimum, Maximum = maximum, Step = step, Choices = c.Pointer
        };
        if (api->DefineSetting(api, &definition) != 0)
            throw new InvalidOperationException($"STRATUM rejected setting '{key}'; see mod logs.");
    }

    public string GetString(string key)
    {
        Span<byte> name = stackalloc byte[Utf8.Size(key, 96)];
        Utf8.Write(key, name);
        Span<byte> value = stackalloc byte[1025];
        fixed (byte* k = name)
        fixed (byte* v = value)
        {
            uint size = api->GetSetting(api, k, v, (uint)value.Length);
            if (size == 0) throw new KeyNotFoundException($"Setting '{key}' is not defined.");
            if (size > value.Length || v[size - 1] != 0)
                throw new InvalidOperationException("Invalid setting buffer from host.");
            return Utf8.Encoding.GetString(value[..((int)size - 1)]);
        }
    }

    public bool GetBool(string key) => GetString(key) switch
    {
        "true" => true, "false" => false,
        _ => throw new FormatException($"Setting '{key}' is not a toggle.")
    };
    public double GetNumber(string key) => double.Parse(GetString(key), CultureInfo.InvariantCulture);
    public int GetKey(string key) => int.Parse(GetString(key), CultureInfo.InvariantCulture);
}
