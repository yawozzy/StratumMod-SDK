param([Parameter(Mandatory=$true)][string]$Zig,[string]$Output)
$ErrorActionPreference='Stop'
$sdk=(Resolve-Path "$PSScriptRoot/../..").Path
if(!$Output){$Output=Join-Path $sdk 'artifacts/HelloNative-1.0.0.strmod'}
$Output=[IO.Path]::GetFullPath($Output)
if(Test-Path -LiteralPath $Output){throw 'Output exists; use a new name or version'}
$scratch=Join-Path $sdk ('artifacts/build/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch -Force | Out-Null
$env:ZIG_GLOBAL_CACHE_DIR=Join-Path $sdk 'artifacts/zig-cache'
$env:ZIG_LOCAL_CACHE_DIR=Join-Path $scratch 'zig-local-cache'
$dll=Join-Path $scratch 'HelloNative.dll'
& $Zig c++ -target x86_64-windows-gnu -std=c++20 -O2 -shared -static -s "-I$sdk/include" (Join-Path $PSScriptRoot 'HelloNative.cpp') -o $dll
if($LASTEXITCODE){throw 'Native mod build failed'}
& (Join-Path $sdk 'bin/stratum-pack.exe') pack $dll (Join-Path $PSScriptRoot 'manifest.ini') (Join-Path $PSScriptRoot 'logo.png') $Output
if($LASTEXITCODE){throw 'Native package validation failed'}
& (Join-Path $sdk 'bin/stratum-pack.exe') inspect $Output
if($LASTEXITCODE){throw 'Final inspection failed'}
Write-Host "Ready: $Output. Import through STRATUM or copy to STRATUM/Mods."
