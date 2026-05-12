namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SayingServiceEdgeCaseTests
{
    private static (IResourceStore store, SayingService svc) Setup()
    {
        var store = Substitute.For<IResourceStore>();
        return (store, new SayingService(store));
    }

    // --- SaveDraftAsync ---

    [Fact]
    public async Task SaveDraft_EmptyBody()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(1, RevisionKind.Edit, "", null).Returns(1L);

        var revId = await svc.SaveDraftAsync(1, "");

        Assert.Equal(1L, revId);
        await store.Received(1).AppendRevisionAsync(1, RevisionKind.Edit, "", null);
    }

    [Fact]
    public async Task SaveDraft_LargeBody()
    {
        var (store, svc) = Setup();
        var bigBody = new string('X', 100_000);
        store.AppendRevisionAsync(1, RevisionKind.Edit, bigBody, null).Returns(1L);

        var revId = await svc.SaveDraftAsync(1, bigBody);

        Assert.Equal(1L, revId);
    }

    [Fact]
    public async Task SaveDraft_ReturnsRevisionId()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(42L);

        var result = await svc.SaveDraftAsync(1, "body");

        Assert.Equal(42L, result);
    }

    [Fact]
    public async Task SaveDraft_AlwaysCreatesTentativeAct()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(5L);

        await svc.SaveDraftAsync(1, "body");

        await store.Received(1).AppendActAsync(1, 5L, ActKind.Tentative, Arg.Any<string?>());
    }

    // --- CommitAsync ---

    [Fact]
    public async Task Commit_EmptyBody()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(1, RevisionKind.Promote, "", null).Returns(1L);

        var revId = await svc.CommitAsync(1, "");

        Assert.Equal(1L, revId);
        await store.Received(1).AppendRevisionAsync(1, RevisionKind.Promote, "", null);
    }

    [Fact]
    public async Task Commit_WithNote_PassesNoteToAct()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(1L);

        await svc.CommitAsync(1, "body", "Important release");

        await store.Received(1).AppendActAsync(1, 1L, ActKind.Commit, "Important release");
    }

    [Fact]
    public async Task Commit_SupersedeCalledRegardlessOfResult()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(1L);
        store.SupersedeLatestCommitAsync(1, 1L).Returns((long?)null);

        await svc.CommitAsync(1, "body");

        await store.Received(1).SupersedeLatestCommitAsync(1, 1L);
    }

    [Fact]
    public async Task Commit_ReturnsRevisionId()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(99L);

        var result = await svc.CommitAsync(1, "body");

        Assert.Equal(99L, result);
    }

    // --- Sequential operations ---

    [Fact]
    public async Task SaveThenCommit_DifferentRevisionKinds()
    {
        var (store, svc) = Setup();
        var revCounter = 0L;
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(_ => Task.FromResult(++revCounter));

        await svc.SaveDraftAsync(1, "draft");
        await svc.CommitAsync(1, "final");

        await store.Received(1).AppendRevisionAsync(1, RevisionKind.Edit, "draft", null);
        await store.Received(1).AppendRevisionAsync(1, RevisionKind.Promote, "final", null);
    }

    [Fact]
    public async Task MultipleSaves_EachCreatesTentative()
    {
        var (store, svc) = Setup();
        var revCounter = 0L;
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(_ => Task.FromResult(++revCounter));

        await svc.SaveDraftAsync(1, "v1");
        await svc.SaveDraftAsync(1, "v2");
        await svc.SaveDraftAsync(1, "v3");

        await store.Received(3).AppendActAsync(1, Arg.Any<long>(), ActKind.Tentative, Arg.Any<string?>());
    }

    [Fact]
    public async Task MultipleCommits_EachSupersedes()
    {
        var (store, svc) = Setup();
        var revCounter = 0L;
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(_ => Task.FromResult(++revCounter));

        await svc.CommitAsync(1, "v1");
        await svc.CommitAsync(1, "v2");

        await store.Received(2).SupersedeLatestCommitAsync(1, Arg.Any<long>());
    }
}
