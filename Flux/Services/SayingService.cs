namespace Flux.Services;

using Flux.Data;

public sealed class SayingService
{
    private readonly ResourceStore _store;

    public SayingService(ResourceStore store)
    {
        _store = store;
    }

    // Save a draft edit = Revision(Edit) + optional Tentative Act
    public async Task<long> SaveDraftAsync(long resourceId, string body)
    {
        var revId = await _store.AppendRevisionAsync(resourceId, RevisionKind.Edit, body);
        await _store.AppendActAsync(resourceId, revId, ActKind.Tentative);
        return revId;
    }

    // Commit = Revision(Promote) + Commit Act + Supersede previous commit
    public async Task<long> CommitAsync(long resourceId, string body, string? note = null)
    {
        var revId = await _store.AppendRevisionAsync(resourceId, RevisionKind.Promote, body);

        // Mark this revision as committed
        await _store.AppendActAsync(resourceId, revId, ActKind.Commit, note);

        // Supersede earlier commit
        await _store.SupersedeLatestCommitAsync(resourceId, revId);

        return revId;
    }
}
