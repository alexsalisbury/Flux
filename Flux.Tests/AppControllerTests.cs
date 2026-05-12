namespace Flux.Tests;

using Flux;
using Flux.Data;
using Flux.Services;
using NSubstitute;

public class AppControllerTests
{
    private readonly IResourceStore _store = Substitute.For<IResourceStore>();
    private readonly AppController _sut;

    public AppControllerTests()
    {
        _sut = new AppController(_store);
    }

    // --- CreateDraftNoteAsync ---

    [Fact]
    public async Task CreateDraft_CallsNextId()
    {
        _store.NextIdAsync().Returns(42L);

        var id = await _sut.CreateDraftNoteAsync();

        Assert.Equal(42, id);
        await _store.Received(1).NextIdAsync();
    }

    [Fact]
    public async Task CreateDraft_InsertsResource()
    {
        _store.NextIdAsync().Returns(7L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.ResourceId == 7 &&
            r.Kind == ResourceKind.Note &&
            r.Source == "Manual" &&
            r.Title == "New Draft #7"));
    }

    [Fact]
    public async Task CreateDraft_SetsArtifactOnly()
    {
        _store.NextIdAsync().Returns(1L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.ResourceType == ResourceType.ArtifactOnly));
    }

    [Fact]
    public async Task CreateDraft_HasEmptyArtifactBody()
    {
        _store.NextIdAsync().Returns(1L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.Artifact != null &&
            r.Artifact.Body == "" &&
            r.Artifact.Language == "en"));
    }

    [Fact]
    public async Task CreateDraft_HasEmptyCollections()
    {
        _store.NextIdAsync().Returns(1L);

        await _sut.CreateDraftNoteAsync();

        await _store.Received(1).InsertAsync(Arg.Is<Resource>(r =>
            r.Revisions.Count == 0 &&
            r.Acts.Count == 0 &&
            r.Tags.Count == 0));
    }

    // --- SaveDraftAsync delegates to SayingService ---

    [Fact]
    public async Task SaveDraft_CallsEditRevisionAndTentativeAct()
    {
        _store.AppendRevisionAsync(1, RevisionKind.Edit, "body", null).Returns(1L);

        await _sut.SaveDraftAsync(1, "body");

        await _store.Received(1).AppendRevisionAsync(1, RevisionKind.Edit, "body", Arg.Any<long?>());
        await _store.Received(1).AppendActAsync(1, 1, ActKind.Tentative, Arg.Any<string?>());
    }

    // --- CommitAsync delegates to SayingService ---

    [Fact]
    public async Task Commit_CallsPromoteRevisionAndCommitAct()
    {
        _store.AppendRevisionAsync(1, RevisionKind.Promote, "text", null).Returns(2L);
        _store.AppendActAsync(1, 2, ActKind.Commit, null).Returns("abc");

        await _sut.CommitAsync(1, "text");

        await _store.Received(1).AppendRevisionAsync(1, RevisionKind.Promote, "text", Arg.Any<long?>());
        await _store.Received(1).AppendActAsync(1, 2, ActKind.Commit, Arg.Any<string?>());
    }

    [Fact]
    public async Task Commit_SupersedesPriorCommit()
    {
        _store.AppendRevisionAsync(1, RevisionKind.Promote, "text", null).Returns(3L);

        await _sut.CommitAsync(1, "text");

        await _store.Received(1).SupersedeLatestCommitAsync(1, 3);
    }

    // --- SearchAsync ---

    [Fact]
    public async Task Search_Any_ReturnsAllResults()
    {
        var results = new List<Resource> { new Resource { ResourceId = 1, Title = "Test" } };
        _store.TextSearchAsync("hello", 200).Returns(results);

        var found = await _sut.SearchAsync("hello", ActFilter.Any);

        Assert.Single(found);
    }

    [Fact]
    public async Task Search_CommitOnly_FiltersUncommitted()
    {
        var uncommitted = new Resource
        {
            ResourceId = 1,
            Revisions = new List<Revision>
            {
                new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)
            },
            Acts = new List<Act>()
        };
        _store.TextSearchAsync("test", 200).Returns(new List<Resource> { uncommitted });

        var found = await _sut.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Empty(found);
    }

    // --- WeaveAsync ---

    [Fact]
    public async Task Weave_WithNoLinks_DoesNotCreateResources()
    {
        await _sut.WeaveAsync(1, "no links here");

        await _store.DidNotReceive().ResolveOrCreateByTitleAsync(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Weave_WithLink_ResolvesTitle()
    {
        _store.ResolveOrCreateByTitleAsync("Target", false).Returns(99L);

        await _sut.WeaveAsync(1, "see [[Target]] for details");

        await _store.Received(1).ResolveOrCreateByTitleAsync("Target", false);
    }
}
