$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

$PublishDir = Join-Path $ScriptDir "publish\CompactGUI_Portable"
$ProjectPath = Join-Path $ScriptDir "CompactGUI\CompactGUI.vbproj"
$Configuration = 'Release'
$Runtime = 'win-x64'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CompactGUI Portable Publisher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Cleaning old publish files..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
    Write-Host "  Cleaned old publish directory" -ForegroundColor Gray
}

$TempPublishDir = Join-Path $ScriptDir "CompactGUI\bin\publish"
if (Test-Path $TempPublishDir) {
    Remove-Item -Recurse -Force $TempPublishDir
    Write-Host "  Cleaned temporary publish files" -ForegroundColor Gray
}
Write-Host ""

Write-Host "[2/5] Checking .NET SDK version..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  Detected .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  [Error] .NET SDK not detected!" -ForegroundColor Red
    Write-Host "  Please install .NET 9.0 SDK" -ForegroundColor Yellow
    pause
    exit 1
}
Write-Host ""

Write-Host "[3/5] Publishing project..." -ForegroundColor Yellow
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
Write-Host "  Runtime: $Runtime" -ForegroundColor Gray
Write-Host "  Output: $PublishDir" -ForegroundColor Gray
Write-Host ""

$arguments = @(
    'publish'
    $ProjectPath
    '-c', $Configuration
    '-r', $Runtime
    '--self-contained', 'true'
    '-p:PublishSingleFile=true'
    '-p:EnableCompressionInSingleFile=true'
    '-p:DebugType=None'
    '-p:DebugSymbols=false'
    "-p:PublishDir=$PublishDir"
    '-p:IncludeNativeLibrariesForSelfExtract=true'
    '--nologo'
)

& dotnet $arguments

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "[Error] Publish failed!" -ForegroundColor Red
    pause
    exit 1
}
Write-Host "  Publish completed" -ForegroundColor Green
Write-Host ""

Write-Host "[4/5] Copying additional files..." -ForegroundColor Yellow
$licensePath = Join-Path $ScriptDir "LICENSE"
$readmePath = Join-Path $ScriptDir "README.md"

if (Test-Path $licensePath) {
    Copy-Item $licensePath $PublishDir -Force
    Write-Host "  Copied LICENSE file" -ForegroundColor Gray
}

if (Test-Path $readmePath) {
    Copy-Item $readmePath $PublishDir -Force
    Write-Host "  Copied README.md file" -ForegroundColor Gray
}
Write-Host ""

Write-Host "[5/5] Publish completed!" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Publish Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Publish Directory: $PublishDir" -ForegroundColor White
Write-Host ""

Write-Host "Generated executable files:" -ForegroundColor Yellow
Get-ChildItem $PublishDir -Filter "*.exe" | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  $($_.Name) - $size MB" -ForegroundColor Green
}
Write-Host ""

Write-Host "Notes:" -ForegroundColor Yellow
Write-Host "  - CompactGUI.exe is the main program (standalone)" -ForegroundColor Gray
Write-Host "  - All dependencies are packaged into single exe" -ForegroundColor Gray
Write-Host "  - Can be copied to any location and run" -ForegroundColor Gray
Write-Host "  - No .NET Runtime installation required" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
pause
