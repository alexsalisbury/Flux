namespace Flux.Tests;

using Flux.Data;
using NSubstitute;

public class IResourceStoreContractTests
{
    // Verifies that the IResourceStore interface can be fully mocked
    // and that all default parameter values work as expected.

    private readonly IResourceStore _store = Substitute.For<IResourceStore>();

    // --- Default parameters ---

    [Fact]
    public async Task AppendRevisionAsync_BasedOnDefaultsToNull()
    {
        _store.AppendRevisionAsync(1, RevisionKind.Edit, "body").Returns(1L);

        var result = await _store.AppendRevisionAsync(1, RevisionKind.Edit, "body");

        Assert.Equal(1L, result);
        await _store.Received(1).AppendRevisionAsync(1, RevisionKind.Edit, "body", null);
    }

    [Fact]
    public async Task AppendActAsync_NoteDefaultsToNull()
    {
        _store.AppendActAsync(1, 1, ActKind.Commit).Returns("id");

        var result = await _store.AppendActAsync(1, 1, ActKind.Commit);

        Assert.Equal("id", result);
    }

    [Fact]
    public async Task TextSearchAsync_TakeDefaultsTo50()
    {
        _store.TextSearchAsync("q").Returns(new List<Resource>());

        await _store.TextSearchAsync("q");

        await _store.Received(1).TextSearchAsync("q", 50);
    }

    [Fact]
    public async Task CreateTitledResourceAsync_SourceDefaultsToWeaver()
    {
        _store.CreateTitledResourceAsync("Title").Returns(1L);

        await _store.CreateTitledResourceAsync("Title");

        await _store.Received(1).CreateTitledResourceAsync("Title", "Weaver", "en");
    }

    // --- All methods are mockable ---

    [Fact]
    public async Task NextIdAsync_Mockable()
    {
        _store.NextIdAsync().Returns(100L);
        Assert.Equal(100L, await _store.NextIdAsync());
    }

    [Fact]
    public async Task InsertAsync_Mockable()
    {
        var r = new Resource { ResourceId = 1 };
        await _store.InsertAsync(r);
        await _store.Received(1).InsertAsync(r);
    }

    [Fact]
    public async Task GetAsync_Mockable_Found()
    {
        var r = new Resource { ResourceId = 5 };
        _store.GetAsync(5).Returns(r);

        var result = await _store.GetAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result!.ResourceId);
    }

    [Fact]
    public async Task GetAsync_Mockable_NotFound()
    {
        _store.GetAsync(999).Returns((Resource?)null);
        Assert.Null(await _store.GetAsync(999));
    }

    [Fact]
    public async Task FindByTitleAsync_Mockable()
    {
        var r = new Resource { ResourceId = 1, Title = "Test" };
        _store.FindByTitleAsync("Test").Returns(r);

        var result = await _store.FindByTitleAsync("Test");

        Assert.Equal("Test", result!.Title);
    }

    [Fact]
    public async Task FindByTitleAsync_NotFound()
    {
        _store.FindByTitleAsync("Missing").Returns((Resource?)null);
        Assert.Null(await _store.FindByTitleAsync("Missing"));
    }

    [Fact]
    public async Task ResolveOrCreateByTitleAsync_Mockable_Found()
    {
        _store.ResolveOrCreateByTitleAsync("Exists", false).Returns(42L);
        Assert.Equal(42L, await _store.ResolveOrCreateByTitleAsync("Exists", false));
    }

    [Fact]
    public async Task ResolveOrCreateByTitleAsync_Mockable_NotFound()
    {
        _store.ResolveOrCreateByTitleAsync("Missing", false).Returns((long?)null);
        Assert.Null(await _store.ResolveOrCreateByTitleAsync("Missing", false));
    }

    [Fact]
    public async Task SupersedeLatestCommitAsync_Mockable()
    {
        _store.SupersedeLatestCommitAsync(1, 5).Returns(3L);
        Assert.Equal(3L, await _store.SupersedeLatestCommitAsync(1, 5));
    }

    [Fact]
    public async Task SupersedeLatestCommitAsync_NoPrior_ReturnsNull()
    {
        _store.SupersedeLatestCommitAsync(1, 1).Returns((long?)null);
        Assert.Null(await _store.SupersedeLatestCommitAsync(1, 1));
    }

    [Fact]
    public async Task UpsertLinkAsync_Mockable()
    {
        await _store.UpsertLinkAsync(1, "Mentions", 2);
        await _store.Received(1).UpsertLinkAsync(1, "Mentions", 2);
    }
}
