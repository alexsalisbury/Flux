namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class SearchServiceEdgeCaseTests
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

    private static Revision Rev(long revId) =>
        new(revId, RevisionKind.Edit, "body", DateTime.UtcNow, null);

    private static Act MakeAct(long revId, ActKind kind) =>
        new(Guid.NewGuid().ToString("N"), revId, kind, DateTime.UtcNow, null);

    private static SearchService Setup(List<Resource> data)
    {
        var store = Substitute.ForPartsOf<ResourceStore>();
        store.TextSearchAsync(Arg.Any<string>(), Arg.Any<int>())
             .Returns(Task.FromResult(data));
        return new SearchService(store);
    }

    [Fact]
    public async Task CommitOnly_MultipleRevisionsMultipleActs_UsesMaxRevisionId()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2), Rev(3)],
            acts: [
                MakeAct(1, ActKind.Commit),
                MakeAct(2, ActKind.Tentative),
                MakeAct(3, ActKind.Commit)
            ]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Single(results);
    }

    [Fact]
    public async Task CommitOnly_SupersededActOnLatest_NotIncluded()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Supersede)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Drafts_SupersededOnLatest_IncludedAsDraft()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(2, ActKind.Supersede)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.Drafts);

        Assert.Single(results);
    }

    [Fact]
    public async Task CommitOnly_EndorseOnLatest_NotIncluded()
    {
        var res = MakeResource(1,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Endorse)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Any_IncludesResourcesWithNoRevisions()
    {
        var noRevs = MakeResource(1);
        var svc = Setup([noRevs]);

        var results = await svc.SearchAsync("q", ActFilter.Any);

        Assert.Single(results);
    }

    [Fact]
    public async Task CommitOnly_MixedActsOnLatest_CommitPresent_Included()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [
                MakeAct(2, ActKind.Tentative),
                MakeAct(2, ActKind.Commit),
                MakeAct(2, ActKind.Endorse)
            ]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Single(results);
    }

    [Fact]
    public async Task Results_TakeZero_ReturnsEmpty()
    {
        var res = MakeResource(1,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Commit)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly, take: 0);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Drafts_CommitOnOlderRevision_LatestHasNoActs_IncludedAsDraft()
    {
        var res = MakeResource(1,
            revisions: [Rev(1), Rev(2)],
            acts: [MakeAct(1, ActKind.Commit)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.Drafts);

        Assert.Single(results);
    }

    [Fact]
    public async Task CommitOnly_SingleRevisionWithCommit_Included()
    {
        var res = MakeResource(1,
            revisions: [Rev(1)],
            acts: [MakeAct(1, ActKind.Commit)]);

        var svc = Setup([res]);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Single(results);
    }

    [Fact]
    public async Task Results_StableOrderByCreatedUtc()
    {
        var t1 = new DateTime(2025, 1, 1);
        var t2 = new DateTime(2025, 3, 1);
        var t3 = new DateTime(2025, 6, 1);

        var resources = new List<Resource>
        {
            MakeResource(2, [Rev(1)], [MakeAct(1, ActKind.Commit)], t2),
            MakeResource(1, [Rev(1)], [MakeAct(1, ActKind.Commit)], t1),
            MakeResource(3, [Rev(1)], [MakeAct(1, ActKind.Commit)], t3),
        };

        var svc = Setup(resources);
        var results = await svc.SearchAsync("q", ActFilter.CommitOnly);

        Assert.Equal(3, results[0].ResourceId);
        Assert.Equal(2, results[1].ResourceId);
        Assert.Equal(1, results[2].ResourceId);
    }
}
