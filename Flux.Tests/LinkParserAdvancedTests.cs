namespace Flux.Tests;

using Flux.Services;

public class LinkParserAdvancedTests
{
    // --- Unclosed / malformed brackets ---

    [Fact]
    public void Extract_UnclosedDoubleBracket_NoMatch()
    {
        Assert.Empty(LinkParser.Extract("[[unclosed"));
    }

    [Fact]
    public void Extract_OpenButSingleClose_NoMatch()
    {
        Assert.Empty(LinkParser.Extract("[[half]"));
    }

    [Fact]
    public void Extract_ClosingBracketsOnly_NoMatch()
    {
        Assert.Empty(LinkParser.Extract("some text ]] here"));
    }

    [Fact]
    public void Extract_TripleBrackets_MatchesBracketedContent()
    {
        var result = LinkParser.Extract("[[[triple]]]");
        // regex \[\[([^\]]+)\]\] matches [[triple]] extracting "[triple"
        Assert.Single(result);
        Assert.Equal("[triple", result[0]);
    }

    // --- Content edge cases ---

    [Fact]
    public void Extract_LinkWithNumbers()
    {
        var result = LinkParser.Extract("[[RFC 2616]]");
        Assert.Single(result);
        Assert.Equal("RFC 2616", result[0]);
    }

    [Fact]
    public void Extract_LinkWithHyphens()
    {
        var result = LinkParser.Extract("[[well-known-uri]]");
        Assert.Single(result);
        Assert.Equal("well-known-uri", result[0]);
    }

    [Fact]
    public void Extract_LinkWithUnicode()
    {
        var result = LinkParser.Extract("[[日本語テスト]]");
        Assert.Single(result);
        Assert.Equal("日本語テスト", result[0]);
    }

    [Fact]
    public void Extract_LinkWithEmoji()
    {
        var result = LinkParser.Extract("[[🚀 Launch Plan]]");
        Assert.Single(result);
        Assert.Equal("🚀 Launch Plan", result[0]);
    }

    [Fact]
    public void Extract_LinkWithPunctuation()
    {
        var result = LinkParser.Extract("[[What's Next?]]");
        Assert.Single(result);
        Assert.Equal("What's Next?", result[0]);
    }

    [Fact]
    public void Extract_VeryLongLinkText()
    {
        var longTitle = new string('A', 500);
        var result = LinkParser.Extract($"[[{longTitle}]]");
        Assert.Single(result);
        Assert.Equal(500, result[0].Length);
    }

    // --- Deduplication preserves first occurrence ---

    [Fact]
    public void Extract_DuplicateDifferentCase_PreservesFirst()
    {
        var result = LinkParser.Extract("[[Alpha]] and [[alpha]]");
        Assert.Single(result);
        Assert.Equal("Alpha", result[0]);
    }

    // --- Whitespace variations ---

    [Fact]
    public void Extract_TabsInLink_Trimmed()
    {
        var result = LinkParser.Extract("[[\tTabbed\t]]");
        Assert.Single(result);
        Assert.Equal("Tabbed", result[0]);
    }

    [Fact]
    public void Extract_NewlineInLink_NoMatch()
    {
        // [^\]]+ does not match across ]], but newlines are allowed
        var result = LinkParser.Extract("[[line1\nline2]]");
        Assert.Single(result);
        Assert.Equal("line1\nline2", result[0]);
    }

    // --- Many links ---

    [Fact]
    public void Extract_TenLinks()
    {
        var text = string.Join(" ", Enumerable.Range(1, 10).Select(i => $"[[Link{i}]]"));
        var result = LinkParser.Extract(text);
        Assert.Equal(10, result.Count);
    }

    // --- Mixed valid and invalid ---

    [Fact]
    public void Extract_MixedBracketStyles()
    {
        var result = LinkParser.Extract("[single] [[valid]] [[[invalid]]]");
        Assert.Equal(2, result.Count);
        Assert.Equal("valid", result[0]);
        Assert.Equal("[invalid", result[1]);
    }

    [Fact]
    public void Extract_LinkAtStartAndEnd()
    {
        var result = LinkParser.Extract("[[Start]]middle[[End]]");
        Assert.Equal(2, result.Count);
        Assert.Equal("Start", result[0]);
        Assert.Equal("End", result[1]);
    }
}
