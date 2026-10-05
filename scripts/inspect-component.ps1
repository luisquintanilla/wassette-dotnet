[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$component = Join-Path $root "src\TextAnalyzer.Component\bin\$Configuration\net10.0\wasi-wasm\native\text-analyzer.wasm"

if (-not (Test-Path $component)) {
    dotnet build (Join-Path $root "src\TextAnalyzer.Component\TextAnalyzer.Component.csproj") --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "The WebAssembly component build failed."
    }
}

if (-not (Get-Command wasm-tools -ErrorAction SilentlyContinue)) {
    throw "wasm-tools is required to inspect the component. Install wasm-tools v1.261.0 or open the devcontainer."
}

wasm-tools component wit $component
