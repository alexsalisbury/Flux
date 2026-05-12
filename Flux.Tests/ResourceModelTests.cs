namespace Flux.Tests;

using Flux.Data;

public class ResourceModelTests
{
    [Fact]
    public void Resource_DefaultValues()
    {
        var res = new Resource();
        Assert.Equal(ResourceType.ArtifactOnly, res.ResourceType);
        Assert.Equal(ResourceKind.Note, res.Kind);
        Assert.Equal("Manual", res.Source);
        Assert.Null(res.Title);
        Assert.Null(res.Artifact);
        Assert.Empty(res.Revisions);
        Assert.Empty(res.Acts);
        Assert.Empty(res.Tags);
        Assert.Empty(res.Links);
    }

    [Fact]
    public void ArtifactBody_RecordEquality()
    {
        var now = DateTime.UtcNow;
        var a = new ArtifactBody("hello", "en", 1, now);
        var b = new ArtifactBody("hello", "en", 1, now);
        Assert.Equal(a, b);
    }

    [Fact]
    public void ArtifactBody_RecordInequality()
    {
        var now = DateTime.UtcNow;
        var a = new ArtifactBody("hello", "en", 1, now);
        var b = new ArtifactBody("world", "en", 1, now);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Revision_RecordEquality()
    {
        var now = DateTime.UtcNow;
        var a = new Revision(1, RevisionKind.Edit, "body", now, null);
        var b = new Revision(1, RevisionKind.Edit, "body", now, null);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Act_RecordEquality()
    {
        var now = DateTime.UtcNow;
        var a = new Act("abc", 1, ActKind.Commit, now, "note");
        var b = new Act("abc", 1, ActKind.Commit, now, "note");
        Assert.Equal(a, b);
    }

    [Fact]
    public void ResourceLink_RecordEquality()
    {
        var a = new ResourceLink("Mentions", 42);
        var b = new ResourceLink("Mentions", 42);
        Assert.Equal(a, b);
    }

    [Fact]
    public void ResourceLink_DifferentType_NotEqual()
    {
        var a = new ResourceLink("Mentions", 42);
        var b = new ResourceLink("References", 42);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Resource_WithInitSyntax_SetsProperties()
    {
        var now = DateTime.UtcNow;
        var res = new Resource
        {
            ResourceId = 7,
            ResourceType = ResourceType.AssetOnly,
            Kind = ResourceKind.WebClip,
            Source = "Import",
            CreatedUtc = now,
            Title = "Test",
            Tags = ["alpha", "beta"],
            Links = [new ResourceLink("Mentions", 99)]
        };

        Assert.Equal(7, res.ResourceId);
        Assert.Equal(ResourceType.AssetOnly, res.ResourceType);
        Assert.Equal(ResourceKind.WebClip, res.Kind);
        Assert.Equal("Import", res.Source);
        Assert.Equal("Test", res.Title);
        Assert.Equal(2, res.Tags.Count);
        Assert.Single(res.Links);
        Assert.Equal(99, res.Links[0].ToResourceId);
    }

    [Theory]
    [InlineData(RevisionKind.Edit, 0)]
    [InlineData(RevisionKind.Promote, 1)]
    public void RevisionKind_HasExpectedValues(RevisionKind kind, int expected)
    {
        Assert.Equal(expected, (int)kind);
    }

    [Theory]
    [InlineData(ActKind.Commit, 0)]
    [InlineData(ActKind.Tentative, 1)]
    [InlineData(ActKind.Supersede, 2)]
    [InlineData(ActKind.Reconsider, 3)]
    [InlineData(ActKind.Endorse, 4)]
    public void ActKind_HasExpectedValues(ActKind kind, int expected)
    {
        Assert.Equal(expected, (int)kind);
    }

    [Theory]
    [InlineData(ResourceType.ArtifactOnly, 0)]
    [InlineData(ResourceType.AssetOnly, 1)]
    public void ResourceType_HasExpectedValues(ResourceType type, int expected)
    {
        Assert.Equal(expected, (int)type);
    }

    [Theory]
    [InlineData(ResourceKind.Note, 0)]
    [InlineData(ResourceKind.WebClip, 1)]
    public void ResourceKind_HasExpectedValues(ResourceKind kind, int expected)
    {
        Assert.Equal(expected, (int)kind);
    }

    [Theory]
    [InlineData(WhisperChannel.STT, 0)]
    [InlineData(WhisperChannel.OCR, 1)]
    [InlineData(WhisperChannel.PDF, 2)]
    [InlineData(WhisperChannel.Other, 3)]
    public void WhisperChannel_HasExpectedValues(WhisperChannel channel, int expected)
    {
        Assert.Equal(expected, (int)channel);
    }

    [Fact]
    public void Whisper_DefaultValues()
    {
        var w = new Whisper();
        Assert.Equal(0, w.ResourceId);
        Assert.Equal(WhisperChannel.STT, w.Channel);
        Assert.Equal("", w.Text);
    }

    [Fact]
    public void Asset_DefaultValues()
    {
        var a = new Asset();
        Assert.Equal(0, a.ResourceId);
        Assert.Equal("", a.ContentType);
        Assert.Equal("", a.StoragePath);
    }
}
