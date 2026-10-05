# Wassette .NET text analyzer

This repository is a small end-to-end example of a C# WebAssembly Component
hosted by [Microsoft Wassette](https://github.com/microsoft/wassette) and
called through the official [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).
The first component is deliberately deterministic and has no filesystem,
network, clock, or environment capabilities.

> **Preview toolchain:** C# component support in
> [componentize-dotnet](https://github.com/bytecodealliance/componentize-dotnet)
> is fast-moving. The pinned package/template versions in this repository are
> part of the build contract. The WIT file is the public interoperability
> contract; keep it compatible when changing the implementation.

## Layout

| Path | Purpose |
| --- | --- |
| `src/TextAnalyzer.Core` | Pure, deterministic analysis logic |
| `src/TextAnalyzer.Component` | Library Wasm Component and exported WIT implementation |
| `src/Wassette.McpClient` | .NET MCP client that launches Wassette over stdio |
| `test/TextAnalyzer.Core.Tests` | Fast unit tests with no external prerequisites |
| `scripts` | Cross-platform build, WIT inspection, and smoke-test helpers |
| `.devcontainer` | Reproducible Linux/Codespaces toolchain |

## Prerequisites

- .NET SDK `10.0.112` (the componentize-dotnet library template currently
  targets .NET 10)
- Access to the `dotnet-experimental` Azure Artifacts feed listed in
  `nuget.config` for NativeAOT LLVM packages
- `wasm-tools` for WIT inspection
- `wassette` `v0.8.0` for the opt-in MCP smoke test

On Windows, install Wassette from the
[official release](https://github.com/microsoft/wassette/releases/tag/v0.8.0)
or WinGet. On Linux, use the matching release archive or the official install
instructions. The included devcontainer installs pinned Linux x64/ARM64
artifacts for Wassette and wasm-tools.

## Build and test

```powershell
dotnet restore Wassette.DotNet.sln
dotnet build Wassette.DotNet.sln --no-restore
dotnet test test\TextAnalyzer.Core.Tests\TextAnalyzer.Core.Tests.csproj --no-restore
```

The component output is normally:

```text
src/TextAnalyzer.Component/bin/Debug/net10.0/wasi-wasm/native/text-analyzer.wasm
```

Inspect the exported component contract:

```powershell
.\scripts\inspect-component.ps1
```

The equivalent Linux command is `./scripts/inspect-component.sh`.

## Wassette MCP smoke test

The smoke test is intentionally opt-in and is not part of `dotnet test`.
It fails with a prerequisite message if `wassette` is not installed:

```powershell
.\scripts\run-smoke.ps1
```

On Linux or in Codespaces:

```bash
./scripts/run-smoke.sh
```

The client starts `wassette run` over stdio by default, loads the component
with a local `file://` URI through the `load-component` tool, lists tools,
invokes the analyzer for valid text, and invokes it again for empty input.
Environment inheritance to the child process is restricted to the MCP SDK's
safe default allowlist plus the runtime variables needed by local .NET
processes. Pass `--http http://127.0.0.1:9001/mcp` to the client to exercise
Wassette's Streamable HTTP mode after starting `wassette serve`.

## WIT contract

`src/TextAnalyzer.Component/component.wit` exports:

```wit
package demo:text-analyzer;

interface analyzer {
  record analysis {
    normalized: string,
    word-count: u32,
    line-count: u32,
    slug: string,
  }

  analyze: func(input: string) -> result<analysis, string>;
}

world text-analyzer {
  export analyzer;
}
```

Wassette turns the documented WIT export into an MCP tool. The component
implementation maps the generated WIT result to the pure core result and does
not request any host capabilities.

## Public-repository notes

No license is selected yet. Choose and add a license before publishing this
scaffold publicly. Contributions should preserve the WIT contract and keep
unit tests independent of external services.
