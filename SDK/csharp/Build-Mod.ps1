param(
    [string]$Project = "$PSScriptRoot/HelloCSharp/HelloCSharp.csproj",
    [string]$DotNet = 'dotnet',
    [string]$Packer,
    [string]$Output,
    [switch]$UseEnvironmentalTools
)
$ErrorActionPreference = 'Stop'
$sdk = (Resolve-Path "$PSScriptRoot/..").Path
$Project = (Resolve-Path -LiteralPath $Project).Path
$projectDirectory = Split-Path -Parent $Project
$manifest = Join-Path $projectDirectory 'manifest.ini'
$versionMatch = [regex]::Match([IO.File]::ReadAllText($manifest), '(?m)^version=([0-9]+\.[0-9]+\.[0-9]+)\r?$')
if (!$versionMatch.Success) { throw 'manifest.ini must contain version=major.minor.patch' }
[xml]$projectXml = [IO.File]::ReadAllText($Project)
$assemblyName = @($projectXml.Project.PropertyGroup.AssemblyName | Where-Object { $_ }) | Select-Object -Last 1
if (!$assemblyName) { $assemblyName = [IO.Path]::GetFileNameWithoutExtension($Project) }
if (!$Packer) { $Packer = Join-Path $sdk 'bin/stratum-pack.exe' }
$Packer = (Resolve-Path -LiteralPath $Packer).Path
if (!$Output) { $Output = Join-Path $sdk "artifacts/$assemblyName-$($versionMatch.Groups[1].Value).strmod" }
$Output = [IO.Path]::GetFullPath($Output)
if (Test-Path -LiteralPath $Output) { throw 'Output exists; use a new filename or version.' }
$scratch = Join-Path $sdk ('artifacts/csharp/' + [Guid]::NewGuid().ToString('N'))
$publish = Join-Path $scratch 'publish'
New-Item -ItemType Directory -Path $scratch -Force | Out-Null
$names = @('DOTNET_CLI_HOME','DOTNET_CLI_TELEMETRY_OPTOUT','DOTNET_SKIP_FIRST_TIME_EXPERIENCE',
    'DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','NUGET_PACKAGES')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    if (!$env:DOTNET_CLI_HOME) { $env:DOTNET_CLI_HOME = Join-Path $sdk 'artifacts/dotnet-home' }
    if (!$env:NUGET_PACKAGES) { $env:NUGET_PACKAGES = Join-Path $sdk 'artifacts/nuget' }
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
    $version = & $DotNet --version
    if ($LASTEXITCODE -or $version -notmatch '^10\.') { throw 'Use the .NET 10 SDK (not only the runtime).' }
    $arguments = @('publish', $Project, '-c', 'Release', '-r', 'win-x64', '--nologo',
        '--artifacts-path', (Join-Path $scratch 'build'), '-o', $publish)
    if ($UseEnvironmentalTools) { $arguments += '-p:IlcUseEnvironmentalTools=true' }
    & $DotNet @arguments
    if ($LASTEXITCODE) { throw 'Native AOT publish failed. Check C++ Build Tools / Windows SDK and AOT warnings.' }
    $dll = Join-Path $publish "$assemblyName.dll"
    if (!(Test-Path -LiteralPath $dll)) { throw "Published DLL not found: $dll" }
    $icon = Join-Path $projectDirectory 'logo.png'
    if (!(Test-Path -LiteralPath $icon)) { $icon = '-' }
    New-Item -ItemType Directory -Path (Split-Path -Parent $Output) -Force | Out-Null
    & $Packer pack $dll $manifest $icon $Output
    if ($LASTEXITCODE) { throw 'Package validation failed. A managed DLL cannot be used instead of Native AOT output.' }
    & $Packer inspect $Output
    if ($LASTEXITCODE) { throw 'Final package inspection failed.' }
    Write-Host "Ready: $Output"
    Write-Host 'Import this .strmod through STRATUM or copy it to STRATUM/Mods. Do not copy bin/obj/publish folders.'
} finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process') }
}
