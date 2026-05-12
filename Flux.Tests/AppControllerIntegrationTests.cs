namespace Flux.Tests;

using Flux;
using Flux.Data;
using Flux.Services;
using NSubstitute;

public class AppControllerIntegrationTests
{
    private readonly IResourceStore _store = Substitute.For<IResourceStore>();
    private readonly AppController _sut;

    public AppControllerIntegrationTests()
    {
        _sut = new AppController(_store);
    }

    [Fact]
    public async Task CreateThenSaveDraft_FlowsCorrectly()
    {
        _store.NextIdAsync().Returns(10L);
        _store.AppendRevisionAsync(10, RevisionKind.Edit, "content", null).Returns(1L);

        var id = await _sut.CreateDraftNoteAsync();
        await _sut.SaveDraftAsync(id, "content");

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r => r.ResourceId == 10));
        await _store.Received(1).AppendRevisionAsync(10, RevisionKind.Edit, "content", null);
        await _store.Received(1).AppendActAsync(10, 1L, ActKind.Tentative, Arg.Any<string?>());
    }

    [Fact]
    public async Task CreateThenCommit_FlowsCorrectly()
    {
        _store.NextIdAsync().Returns(20L);
        _store.AppendRevisionAsync(20, RevisionKind.Promote, "final", null).Returns(1L);
        _store.AppendActAsync(20, 1L, ActKind.Commit, null).Returns("act1");

        var id = await _sut.CreateDraftNoteAsync();
        await _sut.CommitAsync(id, "final");

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r => r.ResourceId == 20));
        await _store.Received(1).AppendActAsync(20, 1L, ActKind.Commit, null);
        await _store.Received(1).SupersedeLatestCommitAsync(20, 1L);
    }

    [Fact]
    public async Task DraftThenCommit_DifferentRevisionKinds()
    {
        _store.NextIdAsync().Returns(1L);
        var revCounter = 0L;
        _store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
              .Returns(_ => Task.FromResult(++revCounter));
        _store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
              .Returns("id");

        await _sut.CreateDraftNoteAsync();
        await _sut.SaveDraftAsync(1, "draft body");
        await _sut.CommitAsync(1, "final body");

        await _store.Received(1).AppendRevisionAsync(1, RevisionKind.Edit, "draft body", null);
        await _store.Received(1).AppendRevisionAsync(1, RevisionKind.Promote, "final body", null);
    }

    [Fact]
    public async Task WeaveAfterCommit_LinksCreated()
    {
        _store.NextIdAsync().Returns(1L);
        _store.AppendRevisionAsync(1, RevisionKind.Promote, "see [[Topic]]", null).Returns(1L);
        _store.AppendActAsync(1, 1L, ActKind.Commit, null).Returns("act1");
        _store.ResolveOrCreateByTitleAsync("Topic", true).Returns(50L);

        await _sut.CreateDraftNoteAsync();
        await _sut.CommitAsync(1, "see [[Topic]]");
        await _sut.WeaveAsync(1, "see [[Topic]]", createMissing: true);

        await _store.Received(1).ResolveOrCreateByTitleAsync("Topic", true);
        await _store.Received(1).UpsertLinkAsync(1, "Mentions", 50);
    }

    [Fact]
    public async Task SearchAfterCommit_FindsCommittedResource()
    {
        var committed = new Resource
        {
            ResourceId = 1,
            CreatedUtc = DateTime.UtcNow,
            Title = "Test",
            Revisions = new() { new Revision(1, RevisionKind.Promote, "body", DateTime.UtcNow, null) },
            Acts = new() { new Act("a1", 1, ActKind.Commit, DateTime.UtcNow, null) },
        };
        _store.TextSearchAsync("Test", 200).Returns(new List<Resource> { committed });

        var found = await _sut.SearchAsync("Test", ActFilter.CommitOnly);

        Assert.Single(found);
        Assert.Equal("Test", found[0].Title);
    }

    [Fact]
    public async Task MultipleDrafts_EachGetsUniqueId()
    {
        var idCounter = 0L;
        _store.NextIdAsync().Returns(_ => Task.FromResult(++idCounter));

        var id1 = await _sut.CreateDraftNoteAsync();
        var id2 = await _sut.CreateDraftNoteAsync();
        var id3 = await _sut.CreateDraftNoteAsync();

        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
        Assert.Equal(3, id3);
        await _store.Received(3).NextIdAsync();
    }

    [Fact]
    public async Task LoadTimeline_ReturnsOrderedData()
    {
        var now = DateTime.UtcNow;
        var resource = new Resource
        {
            ResourceId = 1,
            Revisions = new()
            {
                new(1, RevisionKind.Edit, "v1", now.AddMinutes(-10), null),
                new(2, RevisionKind.Promote, "v2", now, 1),
            },
            Acts = new()
            {
                new("a1", 1, ActKind.Tentative, now.AddMinutes(-10), null),
                new("a2", 2, ActKind.Commit, now, null),
            },
        };
        _store.GetAsync(1).Returns(resource);

        var (revs, acts) = await _sut.LoadTimelineAsync(1);

        Assert.Equal(2, revs.Count);
        Assert.Equal(2, revs[0].RevisionId);
        Assert.Equal(2, acts.Count);
        Assert.Equal("a2", acts[0].ActId);
    }
}
