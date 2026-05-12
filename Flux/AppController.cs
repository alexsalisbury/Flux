namespace Flux;

using Flux.Data;
using Flux.Services;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

public sealed class AppController
{
    private readonly AppServices services;
    private readonly SayingService saying;
    private readonly TimelineService timeline;

    private readonly SearchService search;
    private readonly AutoWeaver weaver;


    public AppController(AppServices appServices)
    {
        services = appServices;
        saying = new SayingService(services.Resources);
        timeline = new TimelineService(services.Resources);

        search = new SearchService(services.Resources);
        weaver = new AutoWeaver(services.Resources);

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
        var id = await services.Resources.NextIdAsync();
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
            Artifact = new ArtifactBody("", "en", 1, now),
            Revisions = new(),
            Acts = new(),
            Tags = new()
        };

        await services.Resources.InsertAsync(res);
        return id;
    }

    public Task<long> SaveDraftAsync(long resourceId, string body) => saying.SaveDraftAsync(resourceId, body);

    public Task<long> CommitAsync(long resourceId, string body, string? note = null) => saying.CommitAsync(resourceId, body, note);

    public Task<(List<Revision> revs, List<Act> acts)> LoadTimelineAsync(long resourceId) => timeline.LoadAsync(resourceId);


    public Task<List<Resource>> SearchAsync(string query, ActFilter act = ActFilter.CommitOnly, int take = 50)
        => search.SearchAsync(query, act, take);

    public Task WeaveAsync(long resourceId, string body, bool createMissing = false)
        => weaver.WeaveAsync(resourceId, body, createMissing);

}
