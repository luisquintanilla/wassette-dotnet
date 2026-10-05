namespace Demo.TextAnalyzer.Core;

public sealed record Analysis(
    string Normalized,
    uint WordCount,
    uint LineCount,
    string Slug);
