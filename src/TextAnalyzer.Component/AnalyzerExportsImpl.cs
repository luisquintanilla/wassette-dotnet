using Demo.TextAnalyzer.Core;
using TextAnalyzerWorld;

namespace TextAnalyzerWorld.wit.Exports.demo.textAnalyzer;

public sealed class AnalyzerExportsImpl : IAnalyzerExports
{
    public static Result<IAnalyzerExports.Analysis, string> Analyze(string input)
    {
        var result = TextAnalyzer.Analyze(input);

        return result.IsSuccess
            ? Result<IAnalyzerExports.Analysis, string>.Ok(new IAnalyzerExports.Analysis(
                result.Value!.Normalized,
                result.Value.WordCount,
                result.Value.LineCount,
                result.Value.Slug))
            : Result<IAnalyzerExports.Analysis, string>.Err(result.Error!);
    }
}
