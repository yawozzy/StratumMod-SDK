namespace Stratum.NativeAot;

public sealed unsafe class ModContext
{
    private readonly HostApi* api;
    public Settings Settings { get; }

    internal ModContext(HostApi* api)
    {
        this.api = api;
        Settings = new Settings(api);
    }

    public string ConfigDirectory => ReadDirectory(api->ConfigDirectory(api));
    public string ModDirectory => ReadDirectory(api->ModDirectory(api));

    private static string ReadDirectory(char* pointer) =>
        pointer == null ? throw new InvalidOperationException("Directory is unavailable.") : new string(pointer);

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        if ((uint)level > 2) throw new ArgumentOutOfRangeException(nameof(level));
        Span<byte> utf8 = stackalloc byte[Utf8.Size(message, 8192)];
        Utf8.Write(message, utf8);
        fixed (byte* pointer = utf8) api->Log(api, (uint)level, pointer);
    }

    public void DrawText(float x, float y, uint rgba, string text)
    {
        Span<byte> utf8 = stackalloc byte[Utf8.Size(text, 4096)];
        Utf8.Write(text, utf8);
        fixed (byte* pointer = utf8) api->DrawText(api, x, y, rgba, pointer);
    }

    public void DrawRectangle(float x, float y, float width, float height, uint rgba) =>
        api->DrawRectangle(api, x, y, width, height, rgba);
}
