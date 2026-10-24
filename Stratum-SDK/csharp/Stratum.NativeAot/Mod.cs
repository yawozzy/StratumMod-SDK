namespace Stratum.NativeAot;

public readonly record struct RenderFrame(float DeltaSeconds, float Width, float Height);

public abstract class Mod
{
    public ModContext Host { get; internal set; } = null!;

    public abstract void OnLoad();
    public virtual bool OnUnload() => true;
    public virtual void OnTick(float deltaSeconds) { }
    public virtual void OnRender(RenderFrame frame) { }
    public virtual void OnSettingChanged(string key, string value) { }
}

public enum LogLevel : uint { Info = 0, Warning = 1, Error = 2 }

public static class Color
{
    public static uint Rgba(byte red, byte green, byte blue, byte alpha = 255) =>
        (uint)red | (uint)green << 8 | (uint)blue << 16 | (uint)alpha << 24;
}
