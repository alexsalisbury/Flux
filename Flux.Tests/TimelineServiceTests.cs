namespace Flux.Tests;

using Flux.Data;
using Flux.Services;
using NSubstitute;

public class TimelineServiceTests
{
    private static (ResourceStore store, TimelineService svc) Setup()
    {
        var store = Substitute.ForPartsOf<ResourceStore>();
        store.WhenForAnyArgs(s => s.GetAsync(default)).DoNotCallBase();
        return (store, new TimelineService(store));
    }

    [Fact]
    public async Task LoadAsync_NullResource_ReturnsEmptyLists()
    {
        var (store, svc) = Setup();
        store.GetAsync(999).Returns(Task.FromResult<Resource?>(null));

        var (revs, acts) = await svc.LoadAsync(999);

        Assert.Empty(revs);
        Assert.Empty(acts);
    }

    [Fact]
    public async Task LoadAsync_ReturnsRevisionsOrderedByDateDescending()
    {
        var (store, svc) = Setup();
        var old = new Revision(1, RevisionKind.Edit, "v1", new DateTime(2025, 1, 1), null);
        var recent = new Revision(2, RevisionKind.Promote, "v2", new DateTime(2025, 6, 1), 1);
        var resource = new Resource
        {
            ResourceId = 1,
            Revisions = [old, recent],
            Acts = [],
        };
        store.GetAsync(1).Returns(Task.FromResult<Resource?>(resource));

        var (revs, _) = await svc.LoadAsync(1);

        Assert.Equal(2, revs.Count);
        Assert.Equal(2, revs[0].RevisionId);
        Assert.Equal(1, revs[1].RevisionId);
    }

    [Fact]
    public async Task LoadAsync_ReturnsActsOrderedByDateDescending()
    {
        var (store, svc) = Setup();
        var earlyAct = new Act("a1", 1, ActKind.Tentative, new DateTime(2025, 1, 1), null);
        var lateAct = new Act("a2", 2, ActKind.Commit, new DateTime(2025, 6, 1), "committed");
        var resource = new Resource
        {
            ResourceId = 1,
            Revisions = [new Revision(1, RevisionKind.Edit, "v1", DateTime.UtcNow, null)],
            Acts = [earlyAct, lateAct],
        };
        store.GetAsync(1).Returns(Task.FromResult<Resource?>(resource));

        var (_, acts) = await svc.LoadAsync(1);

        Assert.Equal(2, acts.Count);
        Assert.Equal("a2", acts[0].ActId);
        Assert.Equal("a1", acts[1].ActId);
    }

    [Fact]
    public async Task LoadAsync_EmptyRevisionsAndActs_ReturnsEmptyLists()
    {
        var (store, svc) = Setup();
        var resource = new Resource
        {
            ResourceId = 1,
            Revisions = [],
            Acts = [],
        };
        store.GetAsync(1).Returns(Task.FromResult<Resource?>(resource));

        var (revs, acts) = await svc.LoadAsync(1);

        Assert.Empty(revs);
        Assert.Empty(acts);
    }
}
