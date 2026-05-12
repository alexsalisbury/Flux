namespace Flux.Data;

public interface IResourceStore
{
    Task<long> NextIdAsync();
    Task InsertAsync(Resource res);
    Task<Resource?> GetAsync(long id);
    Task<long> AppendRevisionAsync(long resourceId, RevisionKind kind, string body, long? basedOn = null);
    Task<string> AppendActAsync(long resourceId, long revisionId, ActKind kind, string? note = null);
    Task<long?> SupersedeLatestCommitAsync(long resourceId, long currentRevisionId);
    Task<List<Resource>> TextSearchAsync(string query, int take = 50);
    Task<Resource?> FindByTitleAsync(string title);
    Task<long> CreateTitledResourceAsync(string title, string source = "Weaver", string? language = "en");
    Task<long?> ResolveOrCreateByTitleAsync(string title, bool createIfMissing);
    Task UpsertLinkAsync(long fromResourceId, string linkType, long toResourceId);
}
