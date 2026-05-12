namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class AutoWeaverEdgeCaseTests
{
    [Fact]
    public async Task WeaveAsync_PartialResolution_OnlyLinksResolvedTitles()
    {
        var store = Substitute.For<IResourceStore>();
        store.ResolveOrCreateByTitleAsync("Found", false).Returns(Task.FromResult<long?>(10));
        store.ResolveOrCreateByTitleAsync("Missing", false).Returns(Task.FromResult<long?>(null));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        var weaver = new AutoWeaver(store);
        await weaver.WeaveAsync(1, "See [[Found]] and [[Missing]].", createMissing: false);

        await store.Received(1).UpsertLinkAsync(1, "Mentions", 10);
        await store.DidNotReceive().UpsertLinkAsync(1, Arg.Any<string>(), Arg.Is<long>(x => x != 10));
    }

    [Fact]
    public async Task WeaveAsync_ManyLinks_ResolvesAllInOrder()
    {
        var store = Substitute.For<IResourceStore>();
        var resolveOrder = new List<string>();

        store.ResolveOrCreateByTitleAsync(Arg.Any<string>(), Arg.Any<bool>())
             .Returns(ci =>
             {
                 var title = ci.ArgAt<string>(0);
                 resolveOrder.Add(title);
                 return Task.FromResult<long?>(title.GetHashCode());
             });
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        var weaver = new AutoWeaver(store);
        await weaver.WeaveAsync(1, "[[Alpha]] then [[Beta]] then [[Gamma]].");

        Assert.Equal(["Alpha", "Beta", "Gamma"], resolveOrder);
    }

    [Fact]
    public async Task WeaveAsync_LinkToSelf_StillCreatesEdge()
    {
        var store = Substitute.For<IResourceStore>();
        store.ResolveOrCreateByTitleAsync("Self", false).Returns(Task.FromResult<long?>(1));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        var weaver = new AutoWeaver(store);
        await weaver.WeaveAsync(1, "Reference to [[Self]].", createMissing: false);

        await store.Received(1).UpsertLinkAsync(1, "Mentions", 1);
    }

    [Fact]
    public async Task WeaveAsync_DefaultCreateMissing_IsFalse()
    {
        var store = Substitute.For<IResourceStore>();
        store.ResolveOrCreateByTitleAsync("Topic", false).Returns(Task.FromResult<long?>(5));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        var weaver = new AutoWeaver(store);
        await weaver.WeaveAsync(1, "See [[Topic]].");

        await store.Received(1).ResolveOrCreateByTitleAsync("Topic", false);
    }

    [Fact]
    public async Task WeaveAsync_WhitespaceInLink_TrimmedBeforeResolve()
    {
        var store = Substitute.For<IResourceStore>();
        store.ResolveOrCreateByTitleAsync("Padded", false).Returns(Task.FromResult<long?>(5));
        store.UpsertLinkAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>()).Returns(Task.CompletedTask);

        var weaver = new AutoWeaver(store);
        await weaver.WeaveAsync(1, "See [[  Padded  ]].");

        await store.Received(1).ResolveOrCreateByTitleAsync("Padded", false);
    }
}
