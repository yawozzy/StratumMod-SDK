# C# Native AOT SDK 0.9.1 — проверка

Дата: 24 октября 2026. Ядро: существующий STRATUM Native 0.9.0, Native ABI 1, без изменений игрового бинарника.

## Сборка

- .NET SDK 10.0.402, Native AOT compiler/runtime packages 10.0.13.
- MSVC x64 linker 14.44, Microsoft Windows SDK x64 package 10.0.26100.9169.
- Нативные тестовые хосты: Zig 0.14.1, Windows x64, C++20.
- C#-обёртка, HelloCSharp и отдельная ошибка-проверка BridgeProbe проходят Native AOT publish без предупреждений; warnings считаются ошибками.
- Инструменты подготовлены отдельно от игры в рабочей папке; в пользовательскую поставку SDK они не включены.

## Пакет

Готовый `HelloCSharp-1.0.0.strmod`: **994312 байт**. SHA-256 всего файла:

```text
21514764c38342ae833b71fa2aac66ccfd1949a6e7ca38966bdbdd437259c63b
```

Проверены Windows x64 PE DLL, отсутствие CLR directory, корректный исполняемый экспорт stratum_mod_main, footer формата 2 / ABI 1, SHA-256, встроенные манифест, PNG и полные тексты лицензий. Прямые импорты содержат только библиотеки Windows: advapi32, bcrypt, kernel32, ole32 и api-ms-win-crt-*. Отдельных импортов .NET/Mono/BepInEx/Stratum.NativeAot.dll нет.

Native AOT включает компоненты runtime внутрь образа; «без отдельного runtime» не означает отсутствие GC или поддержки C# в машинном коде.

## C ABI и устойчивость SDK

Отдельный BridgeProbe собран в Native AOT и вызывается из C++-хоста:

- Проверены размеры Host=72, Plugin=56, Setting=88, Frame=16 и все используемые функции таблицы.
- Неверные ABI/размеры, null entry arguments и отсутствующий callback отвергаются.
- UTF-8 кириллица в настройках, логах и HUD, Windows UTF-16 пути.
- Экземпляр остаётся доступным после 100 вызовов GC.Collect между callbacks.
- Обычные исключения OnLoad, OnTick, OnRender, OnSettingChanged, OnUnload остаются внутри SDK, не пересекают C ABI.
- Повторные вызовы после сбоя не повторяют сообщение об ошибке; OnLoad снимает паузу.
- Некорректная UTF-8 строка callback обрабатывается как ошибка SDK.
- OnUnload=false/exception возвращает запрос перезапуска, true подтверждает отключение.

Оба нативных тестовых файла принудительно включают assert даже при Release/NDEBUG; проверки не исчезают при оптимизации. Тесты запускаются в отдельных процессах: Native AOT DLL не выгружаются через FreeLibrary.

## Интеграция с нативным STRATUM

HelloCSharp загружен настоящим native_host.cpp в изолированном тестовом процессе. Проверены OnLoad/OnTick/OnRender, ненулевой ImGui draw output, четыре типа настроек, сохранение значений и callbacks изменения, отказ записи с откатом UI, неверные/устаревшие запросы, отключение/повторное включение, сохранение настроек и блокировка пакета от замены.

После включения assert в Release-сборке тестового хоста повторно прошли C++ HelloNative, проверки формата/границ/повреждения/managed DLL rejection и подписи существующего ядра. Логика ядра, ABI и установленный winhttp.dll не менялись.

## Готовый публичный SDK

ZIP SDK распакован в отдельную папку. Выдаваемый `csharp/Build-Mod.ps1`, проект примера, ProjectReference на обёртку, embedded notices и **выдаваемый** `bin/stratum-pack.exe` использованы для новой независимой сборки. Полученный пакет успешно прошёл полный native host test с настройками, HUD и отключением. Упаковка не зависит от приватных исходников загрузчика.

Промежуточные bin/obj/artifacts, DLL IL, PDB, NuGet cache, инструменты Microsoft и ключи подписи исключены из ZIP. Руководства и четыре относительные ссылки на иллюстрации поставляются вместе с SDK.

## Реальная CarX Drift Racing Online 2

В собственной изолированной тестовой копии игры одновременно загружены HelloNative и финальный HelloCSharp с указанным SHA-256:

- Оба мода достигли OnLoad и первого tick внутри процесса игры.
- Четыре значения C#-мода сохранены в Configs/stratum.example.csharp/settings.cfg.
- В списке загруженных модулей нет отдельных coreclr.dll, clr.dll, mscoree.dll, Mono, BepInEx и Stratum.NativeAot.dll.
- В активном STRATUM созданы только Configs, Logs и Mods, без извлечённого backend/runtime.
- Тестовый процесс завершён после проверки; основная установка игры и её моды не заменялись.

**Граница проверки:** скрытая фоновая игра не подтвердила Present callback с HUD. Отрисовка и C# render callback проверены отдельно нативным ImGui-хостом; визуальная проверка HUD в активном окне игры остаётся за пользователем. Это не проверка переноса KEFTV, XMetrics или любых старых BepInEx-модов.

## Повторение

В приватном исходном проекте с установленными инструментами:

```powershell
.\tests\Run-AotTests.ps1 -Zig 'D:\Tools\zig\zig.exe' `
  -Packer 'D:\STRATUM-SDK\bin\stratum-pack.exe' `
  -OutputDirectory 'D:\Tests\stratum-aot-fresh'
```

Для проверки структуры/встроенных лицензий без запуска пакета:

```powershell
python .\tests\aot_pe_check.py .\HelloCSharp-1.0.0.strmod --notices .\sdk\csharp\Notices
```

Публичный SDK включает исходники обёртки, оба примера, упаковщик, инструкции и уведомления о лицензиях. Исходники загрузчика, тестовые игровые файлы, SDK Microsoft, промежуточные IL/PDB и закрытые ключи подписи в публичный SDK не входят.
