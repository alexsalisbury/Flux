namespace Flux.Services;

using Flux.Data;

public enum ActFilter { CommitOnly, Drafts, Any }

public sealed class SearchService
{
    private readonly IResourceStore _store;

    public SearchService(IResourceStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Lexical search with Act filter. Default = CommitOnly.
    /// </summary>
    public async Task<List<Resource>> SearchAsync(string query, ActFilter act = ActFilter.CommitOnly, int take = 50)
    {
        var rows = await _store.TextSearchAsync(query, take: 200); // get a larger candidate set

        IEnumerable<Resource> filtered = rows;

        if (act == ActFilter.CommitOnly)
        {
            // Keep resources whose latest revision has a Commit act (or any Commit on latest)
            filtered = rows.Where(r =>
            {
                if (r.Revisions.Count == 0) return false;
                var latestRev = r.Revisions.Max(rv => rv.RevisionId);
                return r.Acts.Any(a => a.RevisionId == latestRev && a.Kind == ActKind.Commit);
            });
        }
        else if (act == ActFilter.Drafts)
        {
            // Drafts = resources whose latest *does not* have a Commit
            filtered = rows.Where(r =>
            {
                if (r.Revisions.Count == 0) return false;
                var latestRev = r.Revisions.Max(rv => rv.RevisionId);
                var latestActs = r.Acts.Where(a => a.RevisionId == latestRev).ToList();
                return latestActs.Count == 0 || !latestActs.Any(a => a.Kind == ActKind.Commit);
            });
        }

        // Order by recency, then trim to "take"
        return filtered
            .OrderByDescending(r => r.CreatedUtc)
            .Take(take)
            .ToList();
    }
}
