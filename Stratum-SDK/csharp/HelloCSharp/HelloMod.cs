using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Stratum.NativeAot;

public static class Exports
{
    [UnmanagedCallersOnly(EntryPoint = "stratum_mod_main", CallConvs = [typeof(CallConvCdecl)])]
    public static int Main(nint host, nint plugin) => ModEntry.Initialize<HelloMod>(host, plugin);
}

public sealed class HelloMod : Mod
{
    private sealed record Options(bool Hud, bool Orange, double Interval);
    private Options options = new(true, false, 10);
    private double elapsed;
    private bool firstTick, firstRender;

    public override void OnLoad()
    {
        var settings = Host.Settings;
        settings.Toggle("hud", "Показывать HUD", true, "Общее", "Панель C#-мода в игровом окне");
        settings.Slider("interval", "Интервал записи", 10, 1, 60, 1, "Логи", "Секунды между сообщениями");
        settings.Choice("style", "Стиль", "Carbon", ["Carbon", "Orange"], "Общее");
        settings.Keybind("hotkey", "Клавиша действия", 119, "Управление", "Пример хранения Windows virtual-key; F8");
        ReadOptions();
        elapsed = 0;
        Host.Log("Hello C#: Native AOT загружен через Native ABI 1");
    }

    private void ReadOptions() => Volatile.Write(ref options, new Options(
        Host.Settings.GetBool("hud"), Host.Settings.GetString("style") == "Orange", Host.Settings.GetNumber("interval")));

    public override void OnSettingChanged(string key, string value)
    {
        ReadOptions();
        Host.Log($"C# setting changed: {key}={value}");
    }

    public override void OnTick(float deltaSeconds)
    {
        if (!firstTick) { Host.Log("First C# AOT tick"); firstTick = true; }
        elapsed += deltaSeconds;
        if (elapsed >= Volatile.Read(ref options).Interval)
        {
            elapsed = 0;
            Host.Log("C# AOT tick active");
        }
    }

    public override void OnRender(RenderFrame frame)
    {
        if (!firstRender) { Host.Log("First C# AOT render callback"); firstRender = true; }
        var current = Volatile.Read(ref options);
        if (!current.Hud) return;
        Host.DrawRectangle(24, 110, 310, 68, current.Orange ? Color.Rgba(85, 47, 24, 235) : Color.Rgba(25, 30, 32, 235));
        Host.DrawText(40, 121, Color.Rgba(255, 157, 81), "STRATUM / Hello C# Native AOT");
        Host.DrawText(40, 148, Color.Rgba(220, 225, 230), "C# source / native .strmod / ABI 1");
    }

    public override bool OnUnload()
    {
        Host.Log("Hello C#: on_unload completed");
        return true;
    }
}
