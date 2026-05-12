namespace Flux.Tests;

using Flux.Services;

public class LinkParserTests
{
    [Fact]
    public void Extract_SingleLink()
    {
        var result = LinkParser.Extract("See [[Project Alpha]] for details.");
        Assert.Single(result);
        Assert.Equal("Project Alpha", result[0]);
    }

    [Fact]
    public void Extract_MultipleLinks()
    {
        var result = LinkParser.Extract("Link to [[Foo]] and [[Bar]] here.");
        Assert.Equal(2, result.Count);
        Assert.Contains("Foo", result);
        Assert.Contains("Bar", result);
    }

    [Fact]
    public void Extract_DuplicatesAreDeduplicated_CaseInsensitive()
    {
        var result = LinkParser.Extract("[[Alpha]] and [[alpha]] and [[ALPHA]]");
        Assert.Single(result);
    }

    [Fact]
    public void Extract_WhitespaceIsTrimmed()
    {
        var result = LinkParser.Extract("[[  padded  ]]");
        Assert.Single(result);
        Assert.Equal("padded", result[0]);
    }

    [Fact]
    public void Extract_EmptyBracketsAreIgnored()
    {
        var result = LinkParser.Extract("[[]] and [[   ]]");
        Assert.Empty(result);
    }

    [Fact]
    public void Extract_NullInput_ReturnsEmpty()
    {
        Assert.Empty(LinkParser.Extract(null!));
    }

    [Fact]
    public void Extract_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(LinkParser.Extract(""));
    }

    [Fact]
    public void Extract_WhitespaceOnly_ReturnsEmpty()
    {
        Assert.Empty(LinkParser.Extract("   "));
    }

    [Fact]
    public void Extract_NoLinks_ReturnsEmpty()
    {
        Assert.Empty(LinkParser.Extract("Just plain text with no links."));
    }

    [Fact]
    public void Extract_NestedBrackets_TakesInnerContent()
    {
        var result = LinkParser.Extract("[[outer [inner] text]]");
        Assert.Empty(result);
    }

    [Fact]
    public void Extract_AdjacentLinks()
    {
        var result = LinkParser.Extract("[[A]][[B]]");
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0]);
        Assert.Equal("B", result[1]);
    }

    [Fact]
    public void Extract_LinkWithSpecialCharacters()
    {
        var result = LinkParser.Extract("[[C# Programming]]");
        Assert.Single(result);
        Assert.Equal("C# Programming", result[0]);
    }

    [Fact]
    public void Extract_MultilineText()
    {
        var text = """
            First line with [[Link1]].
            Second line with [[Link2]].
            """;
        var result = LinkParser.Extract(text);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Extract_SingleBrackets_NotMatched()
    {
        var result = LinkParser.Extract("[not a link]");
        Assert.Empty(result);
    }
}
