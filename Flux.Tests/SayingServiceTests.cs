namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SayingServiceTests
{
    private static (ResourceStore store, SayingService svc) Setup()
    {
        var store = Substitute.ForPartsOf<ResourceStore>();
        return (store, new SayingService(store));
    }

    [Fact]
    public async Task SaveDraftAsync_AppendsEditRevisionAndTentativeAct()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(42, RevisionKind.Edit, "draft body", null)
             .Returns(Task.FromResult(1L));
        store.AppendActAsync(42, 1L, ActKind.Tentative, null)
             .Returns(Task.FromResult("act-id"));

        var revId = await svc.SaveDraftAsync(42, "draft body");

        Assert.Equal(1L, revId);
        await store.Received(1).AppendRevisionAsync(42, RevisionKind.Edit, "draft body", null);
        await store.Received(1).AppendActAsync(42, 1L, ActKind.Tentative, Arg.Any<string?>());
    }

    [Fact]
    public async Task CommitAsync_AppendsPromoteRevisionAndCommitAct()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(10, RevisionKind.Promote, "committed", null)
             .Returns(Task.FromResult(3L));
        store.AppendActAsync(10, 3L, ActKind.Commit, "my note")
             .Returns(Task.FromResult("act-id"));
        store.SupersedeLatestCommitAsync(10, 3L)
             .Returns(Task.FromResult<long?>(2L));

        var revId = await svc.CommitAsync(10, "committed", "my note");

        Assert.Equal(3L, revId);
        await store.Received(1).AppendRevisionAsync(10, RevisionKind.Promote, "committed", null);
        await store.Received(1).AppendActAsync(10, 3L, ActKind.Commit, "my note");
        await store.Received(1).SupersedeLatestCommitAsync(10, 3L);
    }

    [Fact]
    public async Task CommitAsync_WithNoNote_PassesNull()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(Task.FromResult(1L));
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(Task.FromResult("id"));
        store.SupersedeLatestCommitAsync(Arg.Any<long>(), Arg.Any<long>())
             .Returns(Task.FromResult<long?>(null));

        await svc.CommitAsync(1, "body");

        await store.Received(1).AppendActAsync(1, 1L, ActKind.Commit, null);
    }

    [Fact]
    public async Task CommitAsync_SupersedesEarlierCommit()
    {
        var (store, svc) = Setup();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(Task.FromResult(5L));
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(Task.FromResult("id"));
        store.SupersedeLatestCommitAsync(7, 5L)
             .Returns(Task.FromResult<long?>(3L));

        await svc.CommitAsync(7, "new body");

        await store.Received(1).SupersedeLatestCommitAsync(7, 5L);
    }
}
