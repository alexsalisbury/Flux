namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SearchServiceTests
{
    private static Resource MakeResource(
        long id,
        List<Revision>? revisions = null,
        List<Act>? acts = null,
        DateTime? created = null)
    {
        return new Resource
        {
            ResourceId = id,
            CreatedUtc = created ?? DateTime.UtcNow,
            Revisions = revisions ?? new(),
            Acts = acts ?? new(),
        };
    }

    private static Revision Rev(long revId, RevisionKind kind = RevisionKind.Edit) =>
        new(revId, kind, "body", DateTime.UtcNow, null);

    private static Act MakeAct(long revId, ActKind kind) =>
        new(Guid.NewGuid().ToString("N"), revId, kind, DateTime.UtcNow, null);

    private static (IResourceStore store, SearchService svc) Setup(List<Resource> data)
    {
        var store = Substitute.For<IResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(data));
        return (store, new SearchService(store));
    }

    [Fact]
    public async Task CommitOnly_ReturnsOnlyResourcesWithCommitOnLatestRevision()
    {
        var committed = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Commit)]);

        var draft = MakeResource(2,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Tentative)]);

        var (_, svc) = Setup([committed, draft]);

        var results = await svc.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Single(results);
        Assert.Equal(1, results[0].ResourceId);
    }

    [Fact]
    public async Task Drafts_ReturnsOnlyResourcesWithoutCommitOnLatestRevision()
    {
        var committed = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Commit)]);

        var draft = MakeResource(2,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Tentative)]);

        var (_, svc) = Setup([committed, draft]);

        var results = await svc.SearchAsync("test", ActFilter.Drafts);

        Assert.Single(results);
        Assert.Equal(2, results[0].ResourceId);
    }

    [Fact]
    public async Task Any_ReturnsBothCommittedAndDrafts()
    {
        var committed = MakeResource(1,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Commit)]);

        var draft = MakeResource(2,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Tentative)]);

        var (_, svc) = Setup([committed, draft]);

        var results = await svc.SearchAsync("test", ActFilter.Any);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task CommitOnly_SkipsResourcesWithNoRevisions()
    {
        var noRevs = MakeResource(1);

        var (_, svc) = Setup([noRevs]);

        var results = await svc.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Drafts_SkipsResourcesWithNoRevisions()
    {
        var noRevs = MakeResource(1);

        var (_, svc) = Setup([noRevs]);

        var results = await svc.SearchAsync("test", ActFilter.Drafts);

        Assert.Empty(results);
    }

    [Fact]
    public async Task CommitOnly_OlderCommitDoesNotCount()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(1, ActKind.Commit), MakeAct(2, ActKind.Tentative)]);

        var (_, svc) = Setup([res]);

        var results = await svc.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Results_OrderedByCreatedUtcDescending()
    {
        var old = MakeResource(1,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Commit)],
            created: new DateTime(2025, 1, 1));

        var recent = MakeResource(2,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Commit)],
            created: new DateTime(2025, 6, 1));

        var (_, svc) = Setup([old, recent]);

        var results = await svc.SearchAsync("test", ActFilter.CommitOnly);

        Assert.Equal(2, results[0].ResourceId);
        Assert.Equal(1, results[1].ResourceId);
    }

    [Fact]
    public async Task Take_LimitsResults()
    {
        var resources = Enumerable.Range(1, 10).Select(i =>
            MakeResource(i,
                revisions: [Rev(1)],
                acts: [MakeAct(1, ActKind.Commit)],
                created: DateTime.UtcNow.AddMinutes(-i))).ToList();

        var (_, svc) = Setup(resources);

        var results = await svc.SearchAsync("test", ActFilter.CommitOnly, take: 3);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task Drafts_ResourceWithNoActs_IsTreatedAsDraft()
    {
        var noActs = MakeResource(1,
            revisions: [Rev(1)],
            acts: []);

        var (_, svc) = Setup([noActs]);

        var results = await svc.SearchAsync("test", ActFilter.Drafts);

        Assert.Single(results);
        Assert.Equal(1, results[0].ResourceId);
    }
}
