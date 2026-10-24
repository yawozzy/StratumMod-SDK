# STRATUM SDK 0.9.1

**C++ / C# Native AOT · Windows x64 · Native ABI 1 · .strmod v2**

Compatible with STRATUM Native **0.9.0**; no loader update required.

## English

- [C# Native AOT: project, settings, build, installation and troubleshooting](docs/CSHARP-AOT-EN.md).
- [C++: build guide and Native ABI reference](docs/API-EN.md).

C# developers need **.NET 10 SDK, x64 MSVC and Windows SDK**. C++ builds need an x64 C++ compiler; the example was tested with **Zig 0.14.1**. Players need the finished mod package, not the SDK.

## Русский

- [C# Native AOT: проект, настройки, сборка, установка и ошибки](docs/CSHARP-AOT-RU.md).
- [C++: сборка и справка по Native ABI](docs/API-RU.md).
- [Проверенные сценарии и ограничения](docs/CSHARP-AOT-VERIFICATION.md).

Для C# нужны **.NET 10 SDK, x64 MSVC и Windows SDK**. Для C++ нужен компилятор x64; пример проверен с **Zig 0.14.1**. Игроку нужен готовый пакет, SDK в игру не копируется.

## Build / Сборка

Run PowerShell at the SDK root / Откройте PowerShell в корне SDK:

```powershell
# C#
.\csharp\Build-Mod.ps1
# C++
.\examples\HelloNative\Build.ps1 -Zig 'D:\Tools\zig\zig.exe'
```

Output / Результат: **artifacts/*.strmod**. Ready-made examples / Готовые примеры: **examples/*.strmod**.

Close the game before replacing a loaded package. Existing managed DLLs need adaptation and rebuilding; the former Unity/BepInEx API is absent.

Перед заменой загруженного пакета закройте игру. Старые управляемые DLL требуют адаптации и пересборки; прежний Unity/BepInEx API отсутствует.

Loader implementation and private signing keys are excluded. / Исходники загрузчика и закрытые ключи подписи в SDK не входят.
