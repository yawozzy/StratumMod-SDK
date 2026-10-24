# STRATUM C++ SDK and Native ABI

[Русский](API-RU.md) · **SDK 0.9.1 / loader 0.9.0 / Windows x64 / Native ABI 1 / .strmod v2**

For C# source, use the [Native AOT guide](CSHARP-AOT-EN.md). Existing managed STRATUM/BepInEx DLLs and .strmod v1 ZIP packages require source adaptation; renaming a file does not convert it.

## 1. Tools and SDK layout

Use a Windows x64 C++ compiler. The example was tested with **Zig 0.14.1**. MSVC builds should target x64 and use the static CRT (/MT). C++ mod builds do not require .NET or Python.

```text
include/stratum.h                  public C ABI; no import library required
bin/stratum-pack.exe               native packer and inspector
examples/HelloNative/              source, manifest.ini, logo.png, Build.ps1
examples/HelloNative-1.0.0.strmod    ready-to-import C++ example
csharp/                           C# Native AOT wrapper and example project
docs/                             guides and demonstration screenshots
```

Keep the header's default structure alignment. Do not pass STL objects across the ABI.

## 2. Build the example

Open PowerShell at the SDK root:

```powershell
.\examples\HelloNative\Build.ps1 -Zig 'D:\Tools\zig\zig.exe'
```

Output: **artifacts/HelloNative-1.0.0.strmod**. Existing output is not overwritten; use -Output with another filename for subsequent builds.

For your own mod, copy examples/HelloNative to examples/MyMod, then edit **HelloNative.cpp**, **manifest.ini** and **logo.png** inside that folder. Keep the source filename expected by Build.ps1:

```powershell
.\examples\MyMod\Build.ps1 -Zig 'D:\Tools\zig\zig.exe' `
  -Output .\artifacts\MyMod-1.0.0.strmod
```

Manual build and packing:

```powershell
zig c++ -target x86_64-windows-gnu -std=c++20 -O2 -shared -static -s -Iinclude examples/HelloNative/HelloNative.cpp -o MyMod.dll
.\bin\stratum-pack.exe pack MyMod.dll examples/HelloNative/manifest.ini examples/HelloNative/logo.png MyMod-1.0.0.strmod
.\bin\stratum-pack.exe inspect MyMod-1.0.0.strmod
```

Pass **-** instead of the PNG path to omit the logo. The packer checks the native PE and export without running the mod.

## 3. Metadata and resources

Save manifest.ini as UTF-8 without spaces around `=`:

```ini
id=author.my_mod
name=My Native Mod
version=1.0.0
author=Your Name
description=What the mod does and how to use it.
```

Keep the ID unique and stable across updates. Use up to 96 lowercase ASCII letters, digits, `.`, `_` and `-`, starting with a letter or digit. Reserved Windows device names, paths and a trailing dot are rejected. Version has three numeric components. Name and author are limited to 128 UTF-8 bytes each; description to 2048.

The PNG logo may be up to 4 MiB and 4096×4096. Package v2 is a native DLL followed by UTF-8 metadata, PNG bytes and a SHA-256 footer. It is not an encrypted ZIP. One package contains one main DLL. Embed resources at build time or distribute a mod folder with its native dependencies/resources. Folder imports require one valid packaged mod; limits are 512 MiB and 4096 files, without symbolic links or junctions.

## 4. Entry point

Export the exact name **stratum_mod_main**:

```cpp
#include "stratum.h"
static const StratumHost* host;
static int32_t STRATUM_CALL load(void*) {
    host->log(host, 0, "Mod loaded");
    return 0;
}
static int32_t STRATUM_CALL unload(void*) {
    // Stop owned hooks, jobs and subscriptions before returning success.
    return 0;
}
extern "C" STRATUM_EXPORT int32_t STRATUM_CALL stratum_mod_main(
    const StratumHost* api, StratumPlugin* plugin) {
    if (!api || !plugin || api->abi_version != STRATUM_NATIVE_ABI ||
        api->struct_size < sizeof(StratumHost) ||
        plugin->struct_size < sizeof(StratumPlugin)) return 1;
    host = api;
    *plugin = {sizeof(StratumPlugin), STRATUM_NATIVE_ABI,
               nullptr, load, unload, nullptr, nullptr, nullptr};
    return 0;
}
```

Return 0 on success. StratumPlugin supplies user, on_load, on_unload, on_tick, on_render and on_setting_changed. user is an opaque pointer to state you own. Copy temporary callback strings if you need to retain them. Never let exceptions cross the C ABI. Avoid complex initialization and Host API calls in DllMain.

## 5. Lifecycle and threads

- Entry, load, unload, tick and setting callbacks run on STRATUM's worker thread inside the game process.
- Render runs on the game's D3D11 Present thread. HUD can draw while the menu is closed; hidden/inactive game windows may skip callbacks.
- Callbacks for a mod are serialized. Register settings in on_load. Stop owned jobs/threads before confirming on_unload.
- **These are not guaranteed Unity main-thread callbacks.** The SDK does not include game-type wrappers, UnityEngine/IL2CPP managed interop or the old typed event/service APIs.
- Keep callbacks short. Blocking I/O and long waits can delay other mods and rendering.

on_unload=0 confirms deactivation; a nonzero result requires a game restart. The DLL and static state stay mapped until process exit. Re-enabling calls on_load again. A crash does not guarantee cleanup callbacks. Replace loaded packages after closing the game.

## 6. Menu settings and config

Up to 64 fields per mod. StratumSetting includes structure size, key, kind, group, label, description, default_value, numeric bounds and choices. Strings are UTF-8; keys are limited to 96 ASCII characters.

| kind | Valid value |
| --- | --- |
| toggle | "true" or "false" |
| slider | Finite number in range, aligned to step relative to minimum |
| choice | A value from the pipe-separated choices list |
| keybind | Supported Windows virtual-key code, or 0 for unbound |

Keybind accepts A–Z, 0–9, F1–F12, Space, Home, End, PageUp/PageDown or 0. Insert is reserved. The setting stores the key; your mod implements the action.

```cpp
StratumSetting s{sizeof(StratumSetting), "hud", "toggle", "General",
    "Show HUD", "", "true", 0, 0, 0, nullptr};
if (host->define_setting(host, &s)) return 1;
char value[32];
if (host->get_setting(host, "hud", value, sizeof value)) {
    // value is NUL-terminated.
}
```

define_setting returns 0 on success. get_setting returns the required UTF-8 buffer size **including NUL**, or 0 when absent/failed; query with nullptr/capacity=0 if needed. Use a buffer large enough for the returned size. Re-registering a setting during a later on_load preserves its saved value.

Menu edits remain drafts until saved. STRATUM validates and atomically writes **Configs/<id>/settings.cfg**, then calls on_setting_changed. A failed write preserves the live value. Defaults must also be saved to apply.

config_directory returns the mod's Configs/<id> path; mod_directory returns the main DLL's directory. These path pointers are Windows UTF-16 (wchar_t). Store additional data in separate files in the config directory; let STRATUM manage settings.cfg.

## 7. Logs and HUD

host->log takes level 0 (Info), 1 (Warning) or 2 (Error), and up to 8192 UTF-8 bytes. Logs are available in the menu and STRATUM/Logs. Turning off file logging keeps the current in-memory log.

on_render receives StratumFrame with struct_size, delta_seconds, width and height. Draw only within this callback. Coordinates are game-window pixels from the top left; colors use STRATUM_RGBA. ImGui's ABI is not exposed.

```cpp
host->draw_rect(host, 24, 24, 300, 60, STRATUM_RGBA(25,30,32,230));
host->draw_text(host, 40, 40, STRATUM_RGBA(255,157,81,255), "Hello C++");
```

![Mod menu: demonstration data; screenshot uses Russian labels](images/01-mods.png)

![Mod settings: demonstration data; screenshot uses Russian labels](images/02-mod-settings.png)

## 8. Install, update and publish

Import .strmod through **«Установить мод» (Install mod)** or put it in **STRATUM/Mods**. Start the game through Steam and press **Insert**. Check logs, settings, disabling and re-enabling. Close the game before replacing a loaded package.

Release the .strmod and required resource/dependency files. Omit private source, object files and PDBs for a binary-only release. State **STRATUM Native 0.9.0+, Native ABI 1, Windows x64, .strmod v2** as requirements and preserve dependency licenses.

Native mods run with the game's permissions and are not sandboxed. Package SHA-256 detects corruption, not publisher identity. Native code remains reversible. The loader's private signing key and implementation are not included in the SDK.
