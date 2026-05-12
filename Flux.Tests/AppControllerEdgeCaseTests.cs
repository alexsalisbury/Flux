namespace Flux.Tests;

using Flux;
using Flux.Data;
using Flux.Services;
using NSubstitute;

public class AppControllerEdgeCaseTests
{
    private readonly IResourceStore _store = Substitute.For<IResourceStore>();
    private readonly AppController _sut;

    public AppControllerEdgeCaseTests()
    {
        _sut = new AppController(_store);
    }

    // --- CreateDraftNoteAsync edge cases ---

    [Fact]
    public async Task CreateDraft_TitleIncludesId()
    {
        _store.NextIdAsync().Returns(999L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.Title == "New Draft #999"));
    }

    [Fact]
    public async Task CreateDraft_SetsCreatedUtcToRecent()
    {
        _store.NextIdAsync().Returns(1L);
        var before = DateTime.UtcNow;

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.CreatedUtc >= before && r.CreatedUtc <= DateTime.UtcNow));
    }

    [Fact]
    public async Task CreateDraft_ArtifactVersionIsOne()
    {
        _store.NextIdAsync().Returns(1L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.Artifact != null && r.Artifact.Version == 1));
    }

    // --- SearchAsync edge cases ---

    [Fact]
    public async Task Search_CommitOnly_KeepsCommittedResources()
    {
        var committed = new Resource
        {
            ResourceId = 1,
            CreatedUtc = DateTime.UtcNow,
            Revisions = new List<Revision>
            {
                new Revision(1, RevisionKind.Promote, "body", DateTime.UtcNow, null)
            },
            Acts = new List<Act>
            {
                new Act("a1", 1, ActKind.Commit, DateTime.UtcNow, null)
            }
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { committed });

        var found = await _sut.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Single(found);
    }

    [Fact]
    public async Task Search_Drafts_KeepsUncommittedResources()
    {
        var draft = new Resource
        {
            ResourceId = 1,
            CreatedUtc = DateTime.UtcNow,
            Revisions = new List<Revision>
            {
                new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)
            },
            Acts = new List<Act>
            {
                new Act("a1", 1, ActKind.Tentative, DateTime.UtcNow, null)
            }
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { draft });

        var found = await _sut.SearchAsync("test", ActFilter.Drafts);

        Assert.Single(found);
    }

    [Fact]
    public async Task Search_Drafts_ExcludesCommitted()
    {
        var committed = new Resource
        {
            ResourceId = 1,
            Revisions = new List<Revision>
            {
                new Revision(1, RevisionKind.Promote, "body", DateTime.UtcNow, null)
            },
            Acts = new List<Act>
            {
                new Act("a1", 1, ActKind.Commit, DateTime.UtcNow, null)
            }
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { committed });

        var found = await _sut.SearchAsync("test", ActFilter.Drafts);

        Assert.Empty(found);
    }

    [Fact]
    public async Task Search_CommitOnly_ExcludesNoRevisions()
    {
        var noRevs = new Resource
        {
            ResourceId = 1,
            Revisions = new List<Revision>(),
            Acts = new List<Act>()
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { noRevs });

        var found = await _sut.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Empty(found);
    }

    [Fact]
    public async Task Search_Any_ReturnsResourcesWithNoRevisions()
    {
        var noRevs = new Resource { ResourceId = 1 };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { noRevs });

        var found = await _sut.SearchAsync("test", ActFilter.Any);

        Assert.Single(found);
    }

    [Fact]
    public async Task Search_OrdersByCreatedUtcDescending()
    {
        var older = new Resource
        {
            ResourceId = 1,
            CreatedUtc = DateTime.UtcNow.AddDays(-2)
        };
        var newer = new Resource
        {
            ResourceId = 2,
            CreatedUtc = DateTime.UtcNow
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { older, newer });

        var found = await _sut.SearchAsync("test", ActFilter.Any);

        Assert.Equal(2, found[0].ResourceId);
        Assert.Equal(1, found[1].ResourceId);
    }

    // --- WeaveAsync edge cases ---

    [Fact]
    public async Task Weave_MultipleLinks_ResolvesAll()
    {
        _store.ResolveOrCreateByTitleAsync("A", false).Returns(10L);
        _store.ResolveOrCreateByTitleAsync("B", false).Returns(20L);

        await _sut.WeaveAsync(1, "see [[A]] and [[B]]");

        await _store.Received(1).ResolveOrCreateByTitleAsync("A", false);
        await _store.Received(1).ResolveOrCreateByTitleAsync("B", false);
    }

    [Fact]
    public async Task Weave_CreateMissing_PassesTrueToResolve()
    {
        _store.ResolveOrCreateByTitleAsync("New", true).Returns(50L);

        await _sut.WeaveAsync(1, "link to [[New]]", createMissing: true);

        await _store.Received(1).ResolveOrCreateByTitleAsync("New", true);
    }

    [Fact]
    public async Task Weave_DuplicateLinks_ResolvesOnce()
    {
        _store.ResolveOrCreateByTitleAsync("Same", false).Returns(10L);

        await _sut.WeaveAsync(1, "[[Same]] and [[Same]] again");

        await _store.Received(1).UpsertLinkAsync(1, Arg.Any<string>(), 10);
    }
}
