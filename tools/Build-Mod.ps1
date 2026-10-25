<#
.SYNOPSIS
  Builds a Stratum.API mod and packs it into a .strmod.
.DESCRIPTION
  Runs "dotnet build -c Release" for the project, then stratum-pack with the project's manifest.ini and icon.png.
  The .strmod is written next to the project, in its "dist" folder.
.EXAMPLE
  .\tools\Build-Mod.ps1 -Project .\template\MyMod
#>
param(
    [Parameter(Mandatory = $true)][string]$Project,
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$sdk = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$folder = (Resolve-Path $Project).Path
if (Test-Path -LiteralPath $folder -PathType Leaf) { $csproj = $folder; $folder = Split-Path $folder } else { $csproj = Get-ChildItem -LiteralPath $folder -Filter *.csproj | Select-Object -First 1 -ExpandProperty FullName }
if (!$csproj) { throw "No .csproj in $folder" }
if (!(Test-Path (Join-Path $sdk 'refs\UnityEngine.CoreModule.dll'))) { Write-Warning 'Game references are missing: run tools\Get-GameReferences.ps1 -Game "<game folder>" first if the mod uses game classes.' }

$manifest = Join-Path $folder 'manifest.ini'
if (!(Test-Path $manifest)) { throw "manifest.ini not found in $folder" }
$meta = @{}; foreach ($line in Get-Content $manifest) { if ($line -match '^\s*([a-z]+)\s*=\s*(.*)$') { $meta[$matches[1]] = $matches[2].Trim() } }

& dotnet build $csproj -c $Configuration "-p:StratumSdk=$sdk" -nologo -v:q
if ($LASTEXITCODE) { throw 'Build failed' }
[xml]$xml = Get-Content $csproj
$name = ($xml.Project.PropertyGroup | ForEach-Object { $_.AssemblyName } | Where-Object { $_ } | Select-Object -First 1)
if (!$name) { $name = [IO.Path]::GetFileNameWithoutExtension($csproj) }
$dll = Get-ChildItem -Path (Join-Path $folder "bin\$Configuration") -Recurse -Filter "$name.dll" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (!$dll) { throw "Built $name.dll not found" }

$icon = Join-Path $folder 'icon.png'
if (!(Test-Path $icon)) { $icon = Join-Path $folder 'Resources\icon.png' }
if (!(Test-Path $icon)) { $icon = '-' }
$dist = Join-Path $folder 'dist'; New-Item -ItemType Directory -Force $dist | Out-Null
$output = Join-Path $dist "$name-$($meta.version).strmod"
if (Test-Path $output) { Remove-Item $output }
& (Join-Path $sdk 'tools\stratum-pack.exe') pack $dll.FullName $manifest $icon $output
if ($LASTEXITCODE) { throw 'Packing failed' }
Write-Host "Ready: $output"
Write-Host 'Install: copy it to <game>\STRATUM\Mods, or press Insert in the game and use the Install mod button.'
