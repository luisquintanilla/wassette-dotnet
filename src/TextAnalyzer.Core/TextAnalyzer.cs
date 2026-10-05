using System.Text.RegularExpressions;

namespace Demo.TextAnalyzer.Core;

public static partial class TextAnalyzer
{
    public const int MaxInputLength = 10_000;

    public static AnalysisResult Analyze(string? input)
    {
        if (input is null)
        {
            return AnalysisResult.Failure("Input must not be null.");
        }

        if (input.Length > MaxInputLength)
        {
            return AnalysisResult.Failure(
                $"Input exceeds the maximum length of {MaxInputLength} characters.");
        }

        var lines = NormalizeLines(input);
        var firstContentLine = Array.FindIndex(lines, static line => line.Length > 0);
        var lastContentLine = Array.FindLastIndex(lines, static line => line.Length > 0);

        if (firstContentLine < 0)
        {
            return AnalysisResult.Failure("Input must contain at least one non-whitespace character.");
        }

        var contentLines = lines[firstContentLine..(lastContentLine + 1)];
        var normalized = string.Join('\n', contentLines);
        var wordCount = CountWords(normalized);
        var slug = CreateSlug(normalized);

        return AnalysisResult.Success(new Analysis(
            normalized,
            wordCount,
            checked((uint)contentLines.Length),
            slug));
    }

    private static string[] NormalizeLines(string input)
    {
        var withUnixLineEndings = input.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        return withUnixLineEndings
            .Split('\n')
            .Select(static line => WhitespaceRegex().Replace(line.Trim(), " "))
            .ToArray();
    }

    private static uint CountWords(string normalized)
    {
        return checked((uint)WordRegex().Matches(normalized).Count);
    }

    private static string CreateSlug(string normalized)
    {
        var slug = SlugSeparatorRegex().Replace(normalized, "-");
        return slug.Trim('-').ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\S+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"[^\p{L}\p{Nd}]+", RegexOptions.CultureInvariant)]
    private static partial Regex SlugSeparatorRegex();
}
