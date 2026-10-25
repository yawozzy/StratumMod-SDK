<#
.SYNOPSIS
  Extracts the game reference assemblies (Unity, CarX, Il2CppInterop, HarmonyX) from an installed STRATUM.
.DESCRIPTION
  STRATUM 0.12+ carries the C# reference assemblies for the current game version inside winhttp.dll.
  This copies them into the SDK's refs folder so mod projects can compile against the game's classes.
  Run it again after a game or STRATUM update.
.EXAMPLE
  .\tools\Get-GameReferences.ps1 -Game "C:\Program Files (x86)\Steam\steamapps\common\CarX Drift Racing Online 2"
#>
param(
    [Parameter(Mandatory = $true)][string]$Game,
    [string]$Output = (Join-Path $PSScriptRoot '..\refs')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
$core = Join-Path $Game 'winhttp.dll'
if (!(Test-Path -LiteralPath $core)) { throw "winhttp.dll not found in '$Game'. Install STRATUM first." }
$bytes = [IO.File]::ReadAllBytes($core)

# The references are an embedded ZIP. Find its end-of-central-directory record ("PK", 5, 6) and the archive start.
$found = $null
for ($i = $bytes.Length - 22; $i -ge 0 -and !$found; $i--) {
    if ($bytes[$i] -ne 0x50 -or $bytes[$i + 1] -ne 0x4B -or $bytes[$i + 2] -ne 5 -or $bytes[$i + 3] -ne 6) { continue }
    $size = [BitConverter]::ToUInt32($bytes, $i + 12); $offset = [BitConverter]::ToUInt32($bytes, $i + 16); $comment = [BitConverter]::ToUInt16($bytes, $i + 20)
    $start = $i - $size - $offset
    if ($start -lt 0) { continue }
    try {
        $stream = New-Object IO.MemoryStream($bytes, [int]$start, [int]($i + 22 + $comment - $start), $false)
        $zip = New-Object IO.Compression.ZipArchive($stream, [IO.Compression.ZipArchiveMode]::Read)
        if ($zip.GetEntry('UnityEngine.CoreModule.dll')) { $found = $zip } else { $zip.Dispose() }
    } catch { }
}
if (!$found) { throw 'No reference archive in this winhttp.dll. Update STRATUM to 0.12 or newer.' }

New-Item -ItemType Directory -Force -Path $Output | Out-Null
$count = 0
foreach ($entry in $found.Entries) {
    if (!$entry.Name.EndsWith('.dll')) { continue }
    $target = Join-Path $Output $entry.Name
    $in = $entry.Open(); $out = [IO.File]::Create($target)
    try { $in.CopyTo($out) } finally { $out.Dispose(); $in.Dispose() }
    $count++
}
$found.Dispose()
Write-Host "Extracted $count reference assemblies to $(Resolve-Path $Output)"
