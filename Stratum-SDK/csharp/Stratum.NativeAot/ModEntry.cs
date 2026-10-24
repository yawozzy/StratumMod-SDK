using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Stratum.NativeAot;

public static unsafe class ModEntry
{
    private sealed class State(Mod mod)
    {
        internal readonly Mod Mod = mod;
        internal int Faulted;

        internal void Fail(string callback, Exception error)
        {
            if (Interlocked.Exchange(ref Faulted, 1) != 0) return;
            try { Mod.Host.Log($"C# {callback} failed: {error.GetType().Name}: {error.Message}", LogLevel.Error); }
            catch { try { Mod.Host.Log("C# callback failed; SDK callbacks paused.", LogLevel.Error); } catch { } }
        }
    }

    // Called by an UnmanagedCallersOnly export in the mod's own assembly.
    public static int Initialize<T>(nint host, nint plugin) where T : Mod, new()
    {
        var api = (HostApi*)host;
        var output = (PluginApi*)plugin;
        if (!OperatingSystem.IsWindows() || IntPtr.Size != 8 || api == null || output == null ||
            api->Size < sizeof(HostApi) || api->Version != 1 ||
            output->Size < sizeof(PluginApi) || output->Version != 1 ||
            api->Log == null || api->DefineSetting == null || api->GetSetting == null ||
            api->ConfigDirectory == null || api->ModDirectory == null ||
            api->DrawText == null || api->DrawRectangle == null) return 1;
        try
        {
            var mod = new T { Host = new ModContext(api) };
            // Native AOT libraries and the host's module are resident until process exit.
            // Keep the object rooted across OnUnload/OnLoad, as the native host keeps callbacks.
            nint user = GCHandle.ToIntPtr(GCHandle.Alloc(new State(mod)));
            *output = new PluginApi
            {
                Size = (uint)sizeof(PluginApi), Version = 1, User = user,
                Load = &Load, Unload = &Unload, Tick = &Tick, Render = &Render, SettingChanged = &Changed
            };
            return 0;
        }
        catch (Exception error)
        {
            try { new ModContext(api).Log($"C# entry failed: {error.Message}", LogLevel.Error); } catch { }
            return 1;
        }
    }

    private static State Get(nint user) => (State)GCHandle.FromIntPtr(user).Target!;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Load(nint user)
    {
        State? state = null;
        try
        {
            state = Get(user);
            Volatile.Write(ref state.Faulted, 0);
            state.Mod.OnLoad();
            return 0;
        }
        catch (Exception error)
        {
            state?.Fail("OnLoad", error);
            try { state?.Mod.OnUnload(); } catch { }
            return 1;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Unload(nint user)
    {
        State? state = null;
        try { state = Get(user); return state.Mod.OnUnload() ? 0 : 1; }
        catch (Exception error) { state?.Fail("OnUnload", error); return 1; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Tick(nint user, float deltaSeconds)
    {
        State? state = null;
        try { state = Get(user); if (Volatile.Read(ref state.Faulted) == 0) state.Mod.OnTick(deltaSeconds); }
        catch (Exception error) { state?.Fail("OnTick", error); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Render(nint user, NativeFrame* frame)
    {
        State? state = null;
        try
        {
            state = Get(user);
            if (frame != null && frame->Size >= sizeof(NativeFrame) && Volatile.Read(ref state.Faulted) == 0)
                state.Mod.OnRender(new RenderFrame(frame->DeltaSeconds, frame->Width, frame->Height));
        }
        catch (Exception error) { state?.Fail("OnRender", error); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Changed(nint user, byte* key, byte* value)
    {
        State? state = null;
        try
        {
            state = Get(user);
            if (Volatile.Read(ref state.Faulted) == 0)
                state.Mod.OnSettingChanged(Utf8.Read(key, 96), Utf8.Read(value, 1024));
        }
        catch (Exception error) { state?.Fail("OnSettingChanged", error); }
    }
}
