namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SearchServiceTakeTests
{
    private static Resource MakeCommittedResource(long id, DateTime created)
    {
        return new Resource
        {
            ResourceId = id,
            CreatedUtc = created,
            Revisions = new() { new Revision(1, RevisionKind.Promote, "body", created, null) },
            Acts = new() { new Act($"a{id}", 1, ActKind.Commit, created, null) },
        };
    }

    private static SearchService Setup(List<Resource> data)
    {
        var store = Substitute.For<IResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(data));
        return new SearchService(store);
    }

    [Fact]
    public async Task Take_LimitsResults()
    {
        var data = Enumerable.Range(1, 10)
            .Select(i => MakeCommittedResource(i, DateTime.UtcNow.AddMinutes(-i)))
            .ToList();

        var svc = Setup(data);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly, take: 3);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task Take_LargerThanResults_ReturnsAll()
    {
        var data = new List<Resource>
        {
            MakeCommittedResource(1, DateTime.UtcNow),
            MakeCommittedResource(2, DateTime.UtcNow.AddMinutes(-1)),
        };

        var svc = Setup(data);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly, take: 100);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Take_AppliedAfterFiltering()
    {
        var committed = MakeCommittedResource(1, DateTime.UtcNow);
        var draft = new Resource
        {
            ResourceId = 2,
            CreatedUtc = DateTime.UtcNow,
            Revisions = new() { new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null) },
            Acts = new() { new Act("a2", 1, ActKind.Tentative, DateTime.UtcNow, null) },
        };

        var svc = Setup(new List<Resource> { committed, draft });
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly, take: 10);

        Assert.Single(results);
    }

    [Fact]
    public async Task Take_OrderingPreservedBeforeTake()
    {
        var data = Enumerable.Range(1, 5)
            .Select(i => MakeCommittedResource(i, DateTime.UtcNow.AddDays(-i)))
            .ToList();

        var svc = Setup(data);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly, take: 3);

        Assert.Equal(1, results[0].ResourceId);
        Assert.Equal(2, results[1].ResourceId);
        Assert.Equal(3, results[2].ResourceId);
    }
}
