using System.Runtime.InteropServices;

namespace Stratum.NativeAot;

// Layout matches sdk/stratum.h, Native ABI 1. All strings at the ABI are UTF-8,
// except directory pointers, which are Windows UTF-16.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct HostApi
{
    internal uint Size, Version;
    internal nint Context;
    internal delegate* unmanaged[Cdecl]<HostApi*, uint, byte*, void> Log;
    internal delegate* unmanaged[Cdecl]<HostApi*, SettingDefinition*, int> DefineSetting;
    internal delegate* unmanaged[Cdecl]<HostApi*, byte*, byte*, uint, uint> GetSetting;
    internal delegate* unmanaged[Cdecl]<HostApi*, char*> ConfigDirectory;
    internal delegate* unmanaged[Cdecl]<HostApi*, char*> ModDirectory;
    internal delegate* unmanaged[Cdecl]<HostApi*, float, float, uint, byte*, void> DrawText;
    internal delegate* unmanaged[Cdecl]<HostApi*, float, float, float, float, uint, void> DrawRectangle;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct PluginApi
{
    internal uint Size, Version;
    internal nint User;
    internal delegate* unmanaged[Cdecl]<nint, int> Load;
    internal delegate* unmanaged[Cdecl]<nint, int> Unload;
    internal delegate* unmanaged[Cdecl]<nint, float, void> Tick;
    internal delegate* unmanaged[Cdecl]<nint, NativeFrame*, void> Render;
    internal delegate* unmanaged[Cdecl]<nint, byte*, byte*, void> SettingChanged;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeFrame
{
    internal uint Size;
    internal float DeltaSeconds, Width, Height;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SettingDefinition
{
    internal uint Size;
    internal byte* Key;
    internal byte* Kind;
    internal byte* Group;
    internal byte* Label;
    internal byte* Description;
    internal byte* DefaultValue;
    internal double Minimum, Maximum, Step;
    internal byte* Choices;
}
