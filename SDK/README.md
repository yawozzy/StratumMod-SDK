# STRATUM SDK 0.9.1 — C++ и C# Native AOT

Windows x64 / Native ABI 1 / .strmod v2 / совместим с ядром STRATUM Native 0.9.0.
Обновлять winhttp.dll не требуется. Игроку не нужны отдельные .NET/Mono/BepInEx.

- [C# Native AOT: подробный гайд](docs/CSHARP-AOT-RU.md).
- [C++ / базовый Native ABI](docs/API-RU.md).
- [Проверки и ограничения](docs/CSHARP-AOT-VERIFICATION.md).

Сборка C#: `.\csharp\Build-Mod.ps1`. Нужны .NET 10 SDK, x64 MSVC и Windows SDK.
Сборка C++: `.\examples\HelloNative\Build.ps1 -Zig <zig.exe>`.
Готовые примеры находятся в examples/*.strmod; импортируйте один пакет через STRATUM.
Для обновления загруженного пакета закройте игру. SDK и инструменты не копируются в игру.

Старые управляемые DLL требуют адаптации исходников и пересборки. SDK не включает
прежний Unity/BepInEx API. Native AOT компилирует C# в машинный код; это не защита
от любого анализа и не песочница для недоверенных модов.

Исходники самого загрузчика и закрытые ключи подписи в SDK не входят.
