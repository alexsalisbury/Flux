namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class AutoWeaverTests
{
    private static (ResourceStore store, AutoWeaver weaver) Setup()
    {
        var store = Substitute.ForPartsOf<ResourceStore>();
        return (store, new AutoWeaver(store));
    }

    [Fact]
    public async Task WeaveAsync_NoLinks_DoesNothing()
    {
        var (store, weaver) = Setup();

        await weaver.WeaveAsync(1, "No links here.");

        await store.DidNotReceive().ResolveOrCreateByTitleAsync(Arg.Any<string>(), Arg.Any<bool>());
        await store.DidNotReceive().UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>());
    }

    [Fact]
    public async Task WeaveAsync_SingleLink_ResolvesAndCreatesEdge()
    {
        var (store, weaver) = Setup();
        store.ResolveOrCreateByTitleAsync("Target", false)
             .Returns(Task.FromResult<long?>(99));
        store.UpsertLinkAsync(1, "Mentions", 99).Returns(Task.CompletedTask);

        await weaver.WeaveAsync(1, "See [[Target]] for info.", createMissing: false);

        await store.Received(1).ResolveOrCreateByTitleAsync("Target", false);
        await store.Received(1).UpsertLinkAsync(1, "Mentions", 99);
    }

    [Fact]
    public async Task WeaveAsync_MultipleLinks_ResolvesEach()
    {
        var (store, weaver) = Setup();
        store.ResolveOrCreateByTitleAsync("A", false).Returns(Task.FromResult<long?>(10));
        store.ResolveOrCreateByTitleAsync("B", false).Returns(Task.FromResult<long?>(20));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        await weaver.WeaveAsync(1, "Link [[A]] and [[B]].", createMissing: false);

        await store.Received(1).UpsertLinkAsync(1, "Mentions", 10);
        await store.Received(1).UpsertLinkAsync(1, "Mentions", 20);
    }

    [Fact]
    public async Task WeaveAsync_UnresolvedLink_SkipsEdge()
    {
        var (store, weaver) = Setup();
        store.ResolveOrCreateByTitleAsync("Missing", false)
             .Returns(Task.FromResult<long?>(null));

        await weaver.WeaveAsync(1, "See [[Missing]].", createMissing: false);

        await store.Received(1).ResolveOrCreateByTitleAsync("Missing", false);
        await store.DidNotReceive().UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>());
    }

    [Fact]
    public async Task WeaveAsync_CreateMissing_PassesThroughToStore()
    {
        var (store, weaver) = Setup();
        store.ResolveOrCreateByTitleAsync("NewTopic", true)
             .Returns(Task.FromResult<long?>(50));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        await weaver.WeaveAsync(1, "See [[NewTopic]].", createMissing: true);

        await store.Received(1).ResolveOrCreateByTitleAsync("NewTopic", true);
        await store.Received(1).UpsertLinkAsync(1, "Mentions", 50);
    }

    [Fact]
    public async Task WeaveAsync_DuplicateLinks_OnlyProcessedOnce()
    {
        var (store, weaver) = Setup();
        store.ResolveOrCreateByTitleAsync("Same", false)
             .Returns(Task.FromResult<long?>(5));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        await weaver.WeaveAsync(1, "[[Same]] and [[same]] again.", createMissing: false);

        await store.Received(1).ResolveOrCreateByTitleAsync(Arg.Any<string>(), false);
        await store.Received(1).UpsertLinkAsync(1, "Mentions", 5);
    }

    [Fact]
    public async Task WeaveAsync_EmptyBody_DoesNothing()
    {
        var (store, weaver) = Setup();

        await weaver.WeaveAsync(1, "");

        await store.DidNotReceive().ResolveOrCreateByTitleAsync(Arg.Any<string>(), Arg.Any<bool>());
    }
}
