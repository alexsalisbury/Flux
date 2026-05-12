namespace Flux.Tests;

using Flux.Data;
using MongoDB.Bson;

public class AssetWhisperModelTests
{
    // --- Asset record ---

    [Fact]
    public void Asset_DefaultValues()
    {
        var a = new Asset();
        Assert.Equal(0, a.ResourceId);
        Assert.Equal("", a.ContentType);
        Assert.Equal("", a.StoragePath);
    }

    [Fact]
    public void Asset_WithExpression()
    {
        var a = new Asset { ResourceId = 1, ContentType = "image/png", StoragePath = "/blobs/1.png" };
        var b = a with { ContentType = "application/pdf" };

        Assert.Equal("image/png", a.ContentType);
        Assert.Equal("application/pdf", b.ContentType);
        Assert.Equal(a.ResourceId, b.ResourceId);
    }

    [Fact]
    public void Asset_Equality()
    {
        var id = ObjectId.GenerateNewId();
        var a = new Asset { Id = id, ResourceId = 1, ContentType = "text/plain", StoragePath = "/x" };
        var b = new Asset { Id = id, ResourceId = 1, ContentType = "text/plain", StoragePath = "/x" };
        Assert.Equal(a, b);
    }

    [Fact]
    public void Asset_Inequality_DifferentPath()
    {
        var a = new Asset { StoragePath = "/a" };
        var b = new Asset { StoragePath = "/b" };
        Assert.NotEqual(a, b);
    }

    // --- Whisper record ---

    [Fact]
    public void Whisper_DefaultValues()
    {
        var w = new Whisper();
        Assert.Equal(0, w.ResourceId);
        Assert.Equal(WhisperChannel.STT, w.Channel);
        Assert.Equal("", w.Text);
        Assert.Equal(default, w.CreatedUtc);
    }

    [Fact]
    public void Whisper_WithExpression()
    {
        var w = new Whisper { Channel = WhisperChannel.OCR, Text = "hello" };
        var w2 = w with { Channel = WhisperChannel.PDF };

        Assert.Equal(WhisperChannel.OCR, w.Channel);
        Assert.Equal(WhisperChannel.PDF, w2.Channel);
        Assert.Equal("hello", w2.Text);
    }

    [Fact]
    public void Whisper_Equality()
    {
        var id = ObjectId.GenerateNewId();
        var ts = DateTime.UtcNow;
        var a = new Whisper { Id = id, ResourceId = 5, Channel = WhisperChannel.STT, Text = "hi", CreatedUtc = ts };
        var b = new Whisper { Id = id, ResourceId = 5, Channel = WhisperChannel.STT, Text = "hi", CreatedUtc = ts };
        Assert.Equal(a, b);
    }

    [Fact]
    public void Whisper_Inequality_DifferentChannel()
    {
        var a = new Whisper { Channel = WhisperChannel.STT };
        var b = new Whisper { Channel = WhisperChannel.OCR };
        Assert.NotEqual(a, b);
    }

    // --- WhisperChannel enum ---

    [Theory]
    [InlineData(WhisperChannel.STT, 0)]
    [InlineData(WhisperChannel.OCR, 1)]
    [InlineData(WhisperChannel.PDF, 2)]
    [InlineData(WhisperChannel.Other, 3)]
    public void WhisperChannel_HasExpectedValues(WhisperChannel ch, int expected)
    {
        Assert.Equal(expected, (int)ch);
    }
}
