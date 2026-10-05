# Contributing

Keep changes small and deterministic. Run the unit tests before submitting a
change:

```bash
dotnet test test/TextAnalyzer.Core.Tests/TextAnalyzer.Core.Tests.csproj
```

If the component contract changes, also run the component build and
`scripts/inspect-component.*`. Do not add credentials, generated binaries, or
remote-only tests to the repository.
