# Stratum SDK 1.0.0

Requires STRATUM 0.12.0+ · Требуется STRATUM 0.12.0+

## English

- **New: Stratum.API for C# mods.** A normal C# DLL: derive from `StratumMod`, add `[StratumMod]`, override `OnLoad`, `OnUpdate`, `OnDrawUI`, `OnQuit`. Everything runs on the Unity main thread.
- **Mod page in the STRATUM menu.** Buttons, switches, sliders, text fields, drop-downs, progress bars, links, rows, cards and nested pages, drawn in the menu's own style.
- **Hotkeys.** `Input.Bind` with Press, Hold and Release; players rebind them in the menu and the choice is saved.
- **Multiplayer Sync.** Messages between players in the same online room who have the same mod, through the game's own server.
- **Direct game access.** Unity and CarX classes and HarmonyX, as in BepInEx IL2CPP mods; `Get-GameReferences.ps1` takes the references from your STRATUM install.
- **Packing.** `.strmod` now also holds C# mods (name, author, description, logo); `Build-Mod.ps1` builds and packs in one command. Plain `.dll` still works.
- **Template and example.** `template\MyMod` and `examples\ExampleMod`, targeting .NET 8.
- **Guides.** Full guides in Russian and English with screenshots.
- Players need the .NET 8 Runtime x64 for C# mods. The existing C++ and C# Native AOT SDK keeps working unchanged.

## Русский

- **Новое: Stratum.API для C#-модов.** Обычная C# DLL: наследуйте `StratumMod`, добавьте `[StratumMod]`, переопределите `OnLoad`, `OnUpdate`, `OnDrawUI`, `OnQuit`. Всё выполняется в главном потоке Unity.
- **Страница мода в меню STRATUM.** Кнопки, переключатели, слайдеры, поля ввода, списки, прогресс, ссылки, строки, карточки и вложенные страницы — в стиле меню.
- **Горячие клавиши.** `Input.Bind` с режимами нажатие, удержание и отпускание; игрок переназначает их в меню, выбор сохраняется.
- **Мультиплеер (Sync).** Сообщения между игроками в одной онлайн-комнате с этим же модом, через сервер самой игры.
- **Прямой доступ к игре.** Классы Unity и CarX и HarmonyX, как в модах BepInEx IL2CPP; `Get-GameReferences.ps1` берёт ссылки из установленного STRATUM.
- **Упаковка.** `.strmod` теперь подходит и для C#-модов (название, автор, описание, логотип); `Build-Mod.ps1` собирает и пакует одной командой. Обычная `.dll` тоже работает.
- **Шаблон и пример.** `template\MyMod` и `examples\ExampleMod` на .NET 8.
- **Гайды.** Полные руководства на русском и английском со скриншотами.
- Игрокам для C#-модов нужен .NET 8 Runtime x64. Прежний SDK для C++ и C# Native AOT работает как раньше.
