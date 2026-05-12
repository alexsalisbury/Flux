namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SayingServiceOrderingTests
{
    [Fact]
    public async Task SaveDraftAsync_RevisionCreatedBeforeAct()
    {
        var store = Substitute.For<IResourceStore>();
        var callOrder = new List<string>();

        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(ci => { callOrder.Add("revision"); return Task.FromResult(1L); });
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(ci => { callOrder.Add("act"); return Task.FromResult("id"); });

        var svc = new SayingService(store);
        await svc.SaveDraftAsync(1, "body");

        Assert.Equal(["revision", "act"], callOrder);
    }

    [Fact]
    public async Task CommitAsync_RevisionThenActThenSupersede()
    {
        var store = Substitute.For<IResourceStore>();
        var callOrder = new List<string>();

        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(ci => { callOrder.Add("revision"); return Task.FromResult(1L); });
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(ci => { callOrder.Add("act"); return Task.FromResult("id"); });
        store.SupersedeLatestCommitAsync(Arg.Any<long>(), Arg.Any<long>())
             .Returns(ci => { callOrder.Add("supersede"); return Task.FromResult<long?>(null); });

        var svc = new SayingService(store);
        await svc.CommitAsync(1, "body");

        Assert.Equal(["revision", "act", "supersede"], callOrder);
    }

    [Fact]
    public async Task SaveDraftAsync_RevisionIdFlowsToAct()
    {
        var store = Substitute.For<IResourceStore>();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(Task.FromResult(42L));
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(Task.FromResult("id"));

        var svc = new SayingService(store);
        await svc.SaveDraftAsync(10, "body");

        await store.Received(1).AppendActAsync(10, 42L, ActKind.Tentative, Arg.Any<string?>());
    }

    [Fact]
    public async Task CommitAsync_RevisionIdFlowsToActAndSupersede()
    {
        var store = Substitute.For<IResourceStore>();
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(Task.FromResult(77L));
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(Task.FromResult("id"));
        store.SupersedeLatestCommitAsync(Arg.Any<long>(), Arg.Any<long>())
             .Returns(Task.FromResult<long?>(null));

        var svc = new SayingService(store);
        await svc.CommitAsync(5, "body", "note");

        await store.Received(1).AppendActAsync(5, 77L, ActKind.Commit, "note");
        await store.Received(1).SupersedeLatestCommitAsync(5, 77L);
    }

    [Fact]
    public async Task SaveDraftAsync_MultipleCalls_EachGetsOwnRevisionId()
    {
        var store = Substitute.For<IResourceStore>();
        var revCounter = 0L;
        store.AppendRevisionAsync(Arg.Any<long>(), Arg.Any<RevisionKind>(), Arg.Any<string>(), null)
             .Returns(ci => Task.FromResult(++revCounter));
        store.AppendActAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<ActKind>(), Arg.Any<string?>())
             .Returns(Task.FromResult("id"));

        var svc = new SayingService(store);
        var rev1 = await svc.SaveDraftAsync(1, "draft 1");
        var rev2 = await svc.SaveDraftAsync(1, "draft 2");

        Assert.Equal(1L, rev1);
        Assert.Equal(2L, rev2);
        await store.Received(1).AppendActAsync(1, 1L, ActKind.Tentative, Arg.Any<string?>());
        await store.Received(1).AppendActAsync(1, 2L, ActKind.Tentative, Arg.Any<string?>());
    }
}
