namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SearchServiceContractTests
{
    [Fact]
    public async Task SearchAsync_DefaultFilter_IsCommitOnly()
    {
        var store = Substitute.For<IResourceStore>();
        var committed = new Resource
        {
            ResourceId = 1,
            CreatedUtc = DateTime.UtcNow,
            Revisions = [new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)],
            Acts = [new Act("a1", 1, ActKind.Commit, DateTime.UtcNow, null)],
        };
        var draft = new Resource
        {
            ResourceId = 2,
            CreatedUtc = DateTime.UtcNow,
            Revisions = [new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)],
            Acts = [new Act("a2", 1, ActKind.Tentative, DateTime.UtcNow, null)],
        };
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(new List<Resource> { committed, draft }));

        var svc = new SearchService(store);
        var results = await svc.SearchAsync("test");

        Assert.Single(results);
        Assert.Equal(1, results[0].ResourceId);
    }

    [Fact]
    public async Task SearchAsync_PassesHardcodedTake200ToStore()
    {
        var store = Substitute.For<IResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(new List<Resource>()));

        var svc = new SearchService(store);
        await svc.SearchAsync("test", ActFilter.Any, take: 5);

        await store.Received(1).TextSearchAsync("test", 200);
    }

    [Fact]
    public async Task SearchAsync_EmptyStoreResults_ReturnsEmpty()
    {
        var store = Substitute.For<IResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(new List<Resource>()));

        var svc = new SearchService(store);

        var commitOnly = await svc.SearchAsync("q", ActFilter.CommitOnly);
        var drafts = await svc.SearchAsync("q", ActFilter.Drafts);
        var any = await svc.SearchAsync("q", ActFilter.Any);

        Assert.Empty(commitOnly);
        Assert.Empty(drafts);
        Assert.Empty(any);
    }

    [Fact]
    public async Task SearchAsync_PassesQueryToStore()
    {
        var store = Substitute.For<IResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(new List<Resource>()));

        var svc = new SearchService(store);
        await svc.SearchAsync("my specific query");

        await store.Received(1).TextSearchAsync("my specific query", 200);
    }

    [Fact]
    public async Task SearchAsync_DefaultTake_Is50()
    {
        var store = Substitute.For<IResourceStore>();
        var resources = Enumerable.Range(1, 100).Select(i => new Resource
        {
            ResourceId = i,
            CreatedUtc = DateTime.UtcNow.AddMinutes(-i),
            Revisions = [new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)],
            Acts = [new Act($"a{i}", 1, ActKind.Commit, DateTime.UtcNow, null)],
        }).ToList();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(resources));

        var svc = new SearchService(store);
        var results = await svc.SearchAsync("q");

        Assert.Equal(50, results.Count);
    }

    [Theory]
    [InlineData(ActFilter.CommitOnly, 0)]
    [InlineData(ActFilter.Drafts, 1)]
    [InlineData(ActFilter.Any, 2)]
    public void ActFilter_HasExpectedValues(ActFilter filter, int expected)
    {
        Assert.Equal(expected, (int)filter);
    }
}
