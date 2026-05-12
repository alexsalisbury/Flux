namespace Flux.Services;

using Flux.Data;

public sealed class AutoWeaver
{
    private readonly IResourceStore _store;

    public AutoWeaver(IResourceStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Parse [[links]] in the artifact body and ensure Mentions edges exist.
    /// If createMissing=true, create resources for unknown titles.
    /// </summary>
    public async Task WeaveAsync(long resourceId, string body, bool createMissing = false)
    {
        var titles = LinkParser.Extract(body);
        if (titles.Count == 0) return;

        foreach (var t in titles)
        {
            var toId = await _store.ResolveOrCreateByTitleAsync(t, createMissing);
            if (toId is long rid)
            {
                await _store.UpsertLinkAsync(resourceId, "Mentions", rid);
            }
        }
    }
}