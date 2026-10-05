namespace Demo.TextAnalyzer.Core;

public readonly record struct AnalysisResult(
    bool IsSuccess,
    Analysis? Value,
    string? Error)
{
    public static AnalysisResult Success(Analysis value) =>
        new(true, value, null);

    public static AnalysisResult Failure(string error) =>
        new(false, null, error);
}
