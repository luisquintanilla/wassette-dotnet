#!/usr/bin/env bash
set -euo pipefail

configuration="${1:-Debug}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if ! command -v wassette >/dev/null 2>&1; then
  echo "Wassette is required for this smoke test. Install Microsoft Wassette v0.8.0 and ensure 'wassette' is on PATH." >&2
  exit 2
fi

dotnet build "$root/src/TextAnalyzer.Component/TextAnalyzer.Component.csproj" --configuration "$configuration"
component="$root/src/TextAnalyzer.Component/bin/$configuration/net10.0/wasi-wasm/native/text-analyzer.wasm"
if [[ ! -f "$component" ]]; then
  echo "The component build succeeded but '$component' was not produced." >&2
  exit 1
fi

dotnet run --project "$root/src/Wassette.McpClient/Wassette.McpClient.csproj" \
  --configuration "$configuration" -- --component "$component"
