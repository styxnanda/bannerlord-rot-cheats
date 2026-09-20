$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host " Building Realm of Thrones Cheats Mod (RoT 7.1)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$gameDir = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"
$binDir = Join-Path $gameDir "bin\Win64_Shipping_Client"
$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$projectDir = $PSScriptRoot
$sourceDir = Join-Path $projectDir "Source"
$targetDll = Join-Path $projectDir "bin\ROTCheats.dll"
$modulesDir = Join-Path $gameDir "Modules\ROT-Cheats"

if (-not (Test-Path (Join-Path $projectDir "bin"))) {
    New-Item -ItemType Directory -Path (Join-Path $projectDir "bin") | Out-Null
}

$harmonyDll = Join-Path $gameDir "Modules\Bannerlord.Harmony\bin\Win64_Shipping_Client\0Harmony.dll"
if (-not (Test-Path $harmonyDll)) {
    $harmonyDll = "C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\2859188632\bin\Win64_Shipping_Client\0Harmony.dll"
}
$mcmDll = Join-Path $gameDir "Modules\Bannerlord.MBOptionScreen\bin\Win64_Shipping_Client\MCMv5.dll"
if (-not (Test-Path $mcmDll)) {
    $mcmDll = "C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\2859238197\bin\Win64_Shipping_Client\MCMv5.dll"
}
$rotDll = Join-Path $gameDir "Modules\ROT-Core\bin\Win64_Shipping_Client\ROT.dll"
$netstandard = Join-Path $binDir "mono\lib\mono\4.5\Facades\netstandard.dll"

$references = @(
    "/r:`"$netstandard`"",
    "/r:`"$binDir\TaleWorlds.Core.dll`"",
    "/r:`"$binDir\TaleWorlds.MountAndBlade.dll`"",
    "/r:`"$binDir\TaleWorlds.Library.dll`"",
    "/r:`"$binDir\TaleWorlds.CampaignSystem.dll`"",
    "/r:`"$binDir\TaleWorlds.DotNet.dll`"",
    "/r:`"$binDir\TaleWorlds.Engine.dll`"",
    "/r:`"$binDir\TaleWorlds.ObjectSystem.dll`"",
    "/r:`"$binDir\TaleWorlds.SaveSystem.dll`"",
    "/r:`"$binDir\TaleWorlds.InputSystem.dll`"",
    "/r:`"$binDir\TaleWorlds.Localization.dll`"",
    "/r:`"$harmonyDll`"",
    "/r:`"$mcmDll`"",
    "/r:`"$rotDll`"",
    "/r:System.dll",
    "/r:System.Core.dll"
)

$sourceFiles = Get-ChildItem -Path $sourceDir -Filter "*.cs" -Recurse | Select-Object -ExpandProperty FullName
Write-Host "Found $($sourceFiles.Count) source files to compile..." -ForegroundColor Yellow

$compileArgs = @(
    "/target:library",
    "/optimize+",
    "/out:`"$targetDll`"",
    "/nologo"
) + $references + ($sourceFiles | ForEach-Object { "`"$_`"" })

Write-Host "Compiling ROTCheats.dll using $cscPath..." -ForegroundColor Yellow
$proc = Start-Process -FilePath $cscPath -ArgumentList ($compileArgs -join " ") -NoNewWindow -Wait -PassThru

if ($proc.ExitCode -ne 0) {
    Write-Host "`n[ERROR] Compilation failed with exit code $($proc.ExitCode)." -ForegroundColor Red
    exit 1
}

Write-Host "`n[SUCCESS] ROTCheats.dll compiled successfully!" -ForegroundColor Green

# Deploy to Bannerlord Modules
Write-Host "`nDeploying to Bannerlord Modules folder: $modulesDir..." -ForegroundColor Yellow
if (-not (Test-Path "$modulesDir\bin\Win64_Shipping_Client")) {
    New-Item -ItemType Directory -Path "$modulesDir\bin\Win64_Shipping_Client" -Force | Out-Null
}

Copy-Item -Path $targetDll -Destination "$modulesDir\bin\Win64_Shipping_Client\ROTCheats.dll" -Force
Copy-Item -Path (Join-Path $projectDir "SubModule.xml") -Destination "$modulesDir\SubModule.xml" -Force

Write-Host "[DEPLOYED] Module files deployed to $modulesDir." -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host " Mod is ready! Activate 'Realm of Thrones Cheats' in Bannerlord Launcher." -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
