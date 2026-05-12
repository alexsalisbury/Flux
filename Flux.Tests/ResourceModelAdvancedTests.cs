namespace Flux.Tests;

using Flux.Data;
using MongoDB.Bson;

public class ResourceModelAdvancedTests
{
    // --- Resource with-expression ---

    [Fact]
    public void Resource_WithExpression_ChangesTitle()
    {
        var r = new Resource { ResourceId = 1, Title = "Original" };
        var r2 = r with { Title = "Updated" };

        Assert.Equal("Original", r.Title);
        Assert.Equal("Updated", r2.Title);
        Assert.Equal(r.ResourceId, r2.ResourceId);
    }

    [Fact]
    public void Resource_WithExpression_ChangesArtifact()
    {
        var now = DateTime.UtcNow;
        var r = new Resource
        {
            Artifact = new ArtifactBody("v1", "en", 1, now)
        };
        var r2 = r with { Artifact = new ArtifactBody("v2", "en", 2, now) };

        Assert.Equal("v1", r.Artifact.Body);
        Assert.Equal("v2", r2.Artifact!.Body);
        Assert.Equal(2, r2.Artifact.Version);
    }

    // --- Revision chain ---

    [Fact]
    public void Revision_BasedOnRevisionId_TracksLineage()
    {
        var r1 = new Revision(1, RevisionKind.Edit, "draft", DateTime.UtcNow, null);
        var r2 = new Revision(2, RevisionKind.Edit, "refined", DateTime.UtcNow, 1);
        var r3 = new Revision(3, RevisionKind.Promote, "final", DateTime.UtcNow, 2);

        Assert.Null(r1.BasedOnRevisionId);
        Assert.Equal(1, r2.BasedOnRevisionId);
        Assert.Equal(2, r3.BasedOnRevisionId);
    }

    [Fact]
    public void Revision_WithExpression()
    {
        var r = new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null);
        var promoted = r with { Kind = RevisionKind.Promote };

        Assert.Equal(RevisionKind.Edit, r.Kind);
        Assert.Equal(RevisionKind.Promote, promoted.Kind);
        Assert.Equal(r.Body, promoted.Body);
    }

    // --- Act notes ---

    [Fact]
    public void Act_NullNote()
    {
        var a = new Act("id", 1, ActKind.Commit, DateTime.UtcNow, null);
        Assert.Null(a.Note);
    }

    [Fact]
    public void Act_WithNote()
    {
        var a = new Act("id", 1, ActKind.Commit, DateTime.UtcNow, "Release v2.0");
        Assert.Equal("Release v2.0", a.Note);
    }

    [Fact]
    public void Act_WithExpression()
    {
        var a = new Act("id", 1, ActKind.Tentative, DateTime.UtcNow, null);
        var committed = a with { Kind = ActKind.Commit, Note = "approved" };

        Assert.Equal(ActKind.Tentative, a.Kind);
        Assert.Equal(ActKind.Commit, committed.Kind);
        Assert.Equal("approved", committed.Note);
    }

    // --- ResourceLink ---

    [Fact]
    public void ResourceLink_WithExpression()
    {
        var link = new ResourceLink("Mentions", 42);
        var updated = link with { Type = "References" };

        Assert.Equal("Mentions", link.Type);
        Assert.Equal("References", updated.Type);
        Assert.Equal(42, updated.ToResourceId);
    }

    [Fact]
    public void Resource_MultipleLinks()
    {
        var r = new Resource
        {
            Links = new List<ResourceLink>
            {
                new("Mentions", 10),
                new("References", 20),
                new("Mentions", 30),
            }
        };

        Assert.Equal(3, r.Links.Count);
        Assert.Equal(2, r.Links.Count(l => l.Type == "Mentions"));
    }

    // --- Tags ---

    [Fact]
    public void Resource_TagsAreMutable()
    {
        var r = new Resource { Tags = new List<string> { "alpha" } };
        r.Tags.Add("beta");

        Assert.Equal(2, r.Tags.Count);
        Assert.Contains("beta", r.Tags);
    }

    // --- ArtifactBody ---

    [Fact]
    public void ArtifactBody_NullLanguage()
    {
        var a = new ArtifactBody("text", null, 1, DateTime.UtcNow);
        Assert.Null(a.Language);
    }

    [Fact]
    public void ArtifactBody_WithExpression()
    {
        var now = DateTime.UtcNow;
        var a = new ArtifactBody("v1", "en", 1, now);
        var b = a with { Body = "v2", Version = 2 };

        Assert.Equal("v1", a.Body);
        Assert.Equal("v2", b.Body);
        Assert.Equal(2, b.Version);
        Assert.Equal("en", b.Language);
    }

    // --- Resource equality ---

    [Fact]
    public void Resource_SameId_NotEqualDueToListReferences()
    {
        var id = ObjectId.GenerateNewId();
        var now = DateTime.UtcNow;
        var a = new Resource { Id = id, ResourceId = 1, CreatedUtc = now };
        var b = new Resource { Id = id, ResourceId = 1, CreatedUtc = now };
        // Records with mutable List fields compare by reference, not by value
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Resource_SameInstance_Equal()
    {
        var r = new Resource { Id = ObjectId.GenerateNewId(), ResourceId = 1 };
        Assert.Equal(r, r);
    }

    [Fact]
    public void Resource_DifferentId_NotEqual()
    {
        var a = new Resource { Id = ObjectId.GenerateNewId(), ResourceId = 1 };
        var b = new Resource { Id = ObjectId.GenerateNewId(), ResourceId = 1 };
        Assert.NotEqual(a, b);
    }

    // --- Resource with revisions and acts combined ---

    [Fact]
    public void Resource_FullLifecycle()
    {
        var now = DateTime.UtcNow;
        var r = new Resource
        {
            Id = ObjectId.GenerateNewId(),
            ResourceId = 42,
            ResourceType = ResourceType.ArtifactOnly,
            Kind = ResourceKind.Note,
            Source = "Manual",
            CreatedUtc = now,
            Title = "My Note",
            Artifact = new ArtifactBody("Hello world", "en", 3, now),
            Revisions = new()
            {
                new(1, RevisionKind.Edit, "draft", now.AddMinutes(-10), null),
                new(2, RevisionKind.Edit, "revised", now.AddMinutes(-5), 1),
                new(3, RevisionKind.Promote, "Hello world", now, 2),
            },
            Acts = new()
            {
                new("a1", 1, ActKind.Tentative, now.AddMinutes(-10), null),
                new("a2", 2, ActKind.Tentative, now.AddMinutes(-5), null),
                new("a3", 3, ActKind.Commit, now, "Published"),
                new("a4", 2, ActKind.Supersede, now, "Auto-superseded"),
            },
            Tags = ["important", "v1"],
            Links = [new ResourceLink("Mentions", 99)],
        };

        Assert.Equal(3, r.Revisions.Count);
        Assert.Equal(4, r.Acts.Count);
        Assert.Equal(2, r.Tags.Count);
        Assert.Single(r.Links);
        Assert.Equal(3, r.Artifact.Version);

        var latestRev = r.Revisions.Max(rv => rv.RevisionId);
        Assert.Equal(3, latestRev);

        var commitActs = r.Acts.Where(a => a.Kind == ActKind.Commit).ToList();
        Assert.Single(commitActs);
        Assert.Equal("Published", commitActs[0].Note);
    }
}
