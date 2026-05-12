namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class TimelineServiceEdgeCaseTests
{
    [Fact]
    public async Task LoadAsync_SingleRevision_ReturnedInList()
    {
        var store = Substitute.For<IResourceStore>();
        var rev = new Revision(1, RevisionKind.Edit, "only one", new DateTime(2025, 3, 1), null);
        store.GetAsync(1).Returns(Task.FromResult<Resource?>(new Resource
        {
            ResourceId = 1, Revisions = [rev], Acts = [],
        }));

        var svc = new TimelineService(store);
        var (revs, acts) = await svc.LoadAsync(1);

        Assert.Single(revs);
        Assert.Equal("only one", revs[0].Body);
        Assert.Empty(acts);
    }

    [Fact]
    public async Task LoadAsync_ManyRevisions_StableDescendingOrder()
    {
        var store = Substitute.For<IResourceStore>();
        var revisions = Enumerable.Range(1, 5).Select(i =>
            new Revision(i, RevisionKind.Edit, $"v{i}", new DateTime(2025, i, 1), i > 1 ? i - 1 : null)
        ).ToList();

        store.GetAsync(1).Returns(Task.FromResult<Resource?>(new Resource
        {
            ResourceId = 1, Revisions = revisions, Acts = [],
        }));

        var svc = new TimelineService(store);
        var (revs, _) = await svc.LoadAsync(1);

        Assert.Equal(5, revs.Count);
        for (int i = 0; i < revs.Count - 1; i++)
            Assert.True(revs[i].CreatedUtc >= revs[i + 1].CreatedUtc);
    }

    [Fact]
    public async Task LoadAsync_ManyActs_StableDescendingOrder()
    {
        var store = Substitute.For<IResourceStore>();
        var acts = Enumerable.Range(1, 5).Select(i =>
            new Act($"a{i}", 1, ActKind.Tentative, new DateTime(2025, i, 1), null)
        ).ToList();

        store.GetAsync(1).Returns(Task.FromResult<Resource?>(new Resource
        {
            ResourceId = 1,
            Revisions = [new Revision(1, RevisionKind.Edit, "body", DateTime.UtcNow, null)],
            Acts = acts,
        }));

        var svc = new TimelineService(store);
        var (_, result) = await svc.LoadAsync(1);

        Assert.Equal(5, result.Count);
        for (int i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].CreatedUtc >= result[i + 1].CreatedUtc);
    }

    [Fact]
    public async Task LoadAsync_SameTimestamp_AllReturned()
    {
        var store = Substitute.For<IResourceStore>();
        var sameTime = new DateTime(2025, 6, 1);
        var revisions = new List<Revision>
        {
            new(1, RevisionKind.Edit, "a", sameTime, null),
            new(2, RevisionKind.Edit, "b", sameTime, 1),
            new(3, RevisionKind.Promote, "c", sameTime, 2),
        };

        store.GetAsync(1).Returns(Task.FromResult<Resource?>(new Resource
        {
            ResourceId = 1, Revisions = revisions, Acts = [],
        }));

        var svc = new TimelineService(store);
        var (revs, _) = await svc.LoadAsync(1);

        Assert.Equal(3, revs.Count);
    }

    [Fact]
    public async Task LoadAsync_PassesResourceIdToStore()
    {
        var store = Substitute.For<IResourceStore>();
        store.GetAsync(Arg.Any<long>()).Returns(Task.FromResult<Resource?>(null));

        var svc = new TimelineService(store);
        await svc.LoadAsync(42);

        await store.Received(1).GetAsync(42);
    }

    [Fact]
    public async Task LoadAsync_RevisionsAndActsFromDifferentRevisions()
    {
        var store = Substitute.For<IResourceStore>();
        var resource = new Resource
        {
            ResourceId = 1,
            Revisions =
            [
                new Revision(1, RevisionKind.Edit, "draft", new DateTime(2025, 1, 1), null),
                new Revision(2, RevisionKind.Promote, "final", new DateTime(2025, 2, 1), 1),
            ],
            Acts =
            [
                new Act("a1", 1, ActKind.Tentative, new DateTime(2025, 1, 1), null),
                new Act("a2", 2, ActKind.Commit, new DateTime(2025, 2, 1), "shipped"),
                new Act("a3", 1, ActKind.Supersede, new DateTime(2025, 2, 2), "auto"),
            ],
        };
        store.GetAsync(1).Returns(Task.FromResult<Resource?>(resource));

        var svc = new TimelineService(store);
        var (revs, acts) = await svc.LoadAsync(1);

        Assert.Equal(2, revs.Count);
        Assert.Equal(3, acts.Count);
        Assert.Equal("a3", acts[0].ActId);
        Assert.Equal("a2", acts[1].ActId);
        Assert.Equal("a1", acts[2].ActId);
    }
}
