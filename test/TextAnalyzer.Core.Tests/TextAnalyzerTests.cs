using Demo.TextAnalyzer.Core;
using Xunit;

namespace Demo.TextAnalyzer.Core.Tests;

public sealed class TextAnalyzerTests
{
    [Fact]
    public void Analyze_ValidText_ReturnsNormalizedMetadataAndSlug()
    {
        var result = TextAnalyzer.Analyze("  Hello,   world!  \r\nNew line.  ");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal("Hello, world!\nNew line.", result.Value!.Normalized);
        Assert.Equal((uint)4, result.Value.WordCount);
        Assert.Equal((uint)2, result.Value.LineCount);
        Assert.Equal("hello-world-new-line", result.Value.Slug);
    }

    [Fact]
    public void Analyze_MixedLineEndingsAndWhitespace_NormalizesWithoutChangingLineCount()
    {
        var result = TextAnalyzer.Analyze("\r\nFirst\tline\rSecond  line\n\nThird line\r\n");

        Assert.True(result.IsSuccess);
        Assert.Equal("First line\nSecond line\n\nThird line", result.Value!.Normalized);
        Assert.Equal((uint)6, result.Value.WordCount);
        Assert.Equal((uint)4, result.Value.LineCount);
        Assert.Equal("first-line-second-line-third-line", result.Value.Slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Analyze_EmptyOrWhitespaceInput_ReturnsError(string input)
    {
        var result = TextAnalyzer.Analyze(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Input must contain at least one non-whitespace character.", result.Error);
    }

    [Fact]
    public void Analyze_NullInput_ReturnsError()
    {
        var result = TextAnalyzer.Analyze(null);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Input must not be null.", result.Error);
    }

    [Fact]
    public void Analyze_InputAtMaximumLength_IsAcceptedAndAnalyzed()
    {
        var input = "a" + new string(' ', TextAnalyzer.MaxInputLength - 2) + "b";

        var result = TextAnalyzer.Analyze(input);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal("a b", result.Value!.Normalized);
        Assert.Equal((uint)2, result.Value.WordCount);
        Assert.Equal((uint)1, result.Value.LineCount);
        Assert.Equal("a-b", result.Value.Slug);
    }

    [Fact]
    public void Analyze_PunctuationAtSlugBoundaries_TrimsSeparators()
    {
        var result = TextAnalyzer.Analyze("!!!Hello, world!!!");

        Assert.True(result.IsSuccess);
        Assert.Equal("!!!Hello, world!!!", result.Value!.Normalized);
        Assert.Equal((uint)2, result.Value.WordCount);
        Assert.Equal((uint)1, result.Value.LineCount);
        Assert.Equal("hello-world", result.Value.Slug);
    }

    [Fact]
    public void Analyze_InputOverMaximumLength_ReturnsError()
    {
        var result = TextAnalyzer.Analyze(new string('x', TextAnalyzer.MaxInputLength + 1));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            $"Input exceeds the maximum length of {TextAnalyzer.MaxInputLength} characters.",
            result.Error);
    }
}
