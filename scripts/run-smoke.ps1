[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$wassette = Get-Command wassette -ErrorAction SilentlyContinue
if ($null -eq $wassette) {
    throw "Wassette is required for this smoke test. Install Microsoft Wassette v0.8.0 and ensure 'wassette' is on PATH."
}

dotnet build (Join-Path $root "src\TextAnalyzer.Component\TextAnalyzer.Component.csproj") --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "The WebAssembly component build failed."
}

$component = Join-Path $root "src\TextAnalyzer.Component\bin\$Configuration\net10.0\wasi-wasm\native\text-analyzer.wasm"
if (-not (Test-Path $component)) {
    throw "The component build succeeded but '$component' was not produced."
}

dotnet run --project (Join-Path $root "src\Wassette.McpClient\Wassette.McpClient.csproj") --configuration $Configuration -- --component $component
if ($LASTEXITCODE -ne 0) {
    throw "The Wassette MCP smoke test failed."
}
