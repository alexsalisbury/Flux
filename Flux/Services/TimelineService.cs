namespace Flux.Services;

using Flux.Data;

public sealed class TimelineService
{
    private readonly ResourceStore _store;

    public TimelineService(ResourceStore store)
    {
        _store = store;
    }

    public async Task<(List<Revision> revs, List<Act> acts)> LoadAsync(long resourceId)
    {
        var doc = await _store.GetAsync(resourceId);
        if (doc == null)
            return (new(), new());

        return (doc.Revisions.OrderByDescending(r => r.CreatedUtc).ToList(),
                doc.Acts.OrderByDescending(a => a.CreatedUtc).ToList());
    }
}