namespace Flux;

using Flux.Data;
using System.Threading.Tasks;

public sealed class AppController
{
    private readonly AppServices _services;

    public AppController(AppServices services)
    {
        _services = services;
    }

    // --- SEARCH ----------------------------------------------------------

    public Task<IEnumerable<Resource>> SearchAsync(string query)
    {
        // Phase 1: stub
        // Phase 2: real search over Mongo text index
        return Task.FromResult<IEnumerable<Resource>>(Array.Empty<Resource>());
    }

    // --- CAPTURE ---------------------------------------------------------

    public async Task<long> CreateDraftNoteAsync()
    {
        var id = await _services.Resources.NextIdAsync();
        var now = DateTime.UtcNow;

        var res = new Resource
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId(),
            ResourceId = id,
            ResourceType = ResourceType.ArtifactOnly,
            Kind = ResourceKind.Note,
            Source = "Manual",
            CreatedUtc = now,
            Title = $"New Draft #{id}",
            Artifact = new ArtifactBody("", "en", 1, now)
        };

        await _services.Resources.InsertAsync(res);
        return id;
    }

    //// --- TIMELINE --------------------------------------------------------

    //public Task<List<Revision>> GetRevisionsAsync(long resourceId)
    //{
    //    // Phase 1: empty
    //    // Phase 2: fetch res.Revisions
    //    return Task.FromResult(new List<Revision>());
    //}
}
