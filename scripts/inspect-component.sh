#!/usr/bin/env bash
set -euo pipefail

configuration="${1:-Debug}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
component="$root/src/TextAnalyzer.Component/bin/$configuration/net10.0/wasi-wasm/native/text-analyzer.wasm"

if [[ ! -f "$component" ]]; then
  dotnet build "$root/src/TextAnalyzer.Component/TextAnalyzer.Component.csproj" --configuration "$configuration"
fi

if ! command -v wasm-tools >/dev/null 2>&1; then
  echo "wasm-tools is required to inspect the component. Install wasm-tools v1.261.0 or open the devcontainer." >&2
  exit 2
fi

wasm-tools component wit "$component"
