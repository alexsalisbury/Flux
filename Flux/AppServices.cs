namespace Flux;

using Flux.Data;
using Flux.Data.MongoCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System.Threading.Tasks;

public sealed class AppServices : IDisposable
{
    public MongoConnection Conn { get; }
    public MongoCollections Collections { get; }
    public ResourceStore Resources { get; }
    public AssetStore Assets { get; }
    public WhisperStore Whispers { get; }

    public AppServices(IConfigurationRoot configuration)
    {
        var connStr = Environment.GetEnvironmentVariable("FLUX_MONGO")
            ?? configuration["Mongo:ConnectionString"];
        var dbName = Environment.GetEnvironmentVariable("FLUX_MONGO_DB")
            ?? configuration["Mongo:Database"];

        Conn = new MongoConnection(connStr, dbName);
        Collections = new MongoCollections(Conn.Database);

        Resources = new ResourceStore(Collections);
        Assets = new AssetStore(Collections);
        Whispers = new WhisperStore(Collections);
    }

    /// <summary>
    /// Demo: create a new note resource for smoke test
    /// </summary>
    public async Task<long> DemoCreateNoteAsync(string title)
    {
        var id = await Resources.NextIdAsync();
        var now = DateTime.UtcNow;

        var res = new Resource
        {
            Id = ObjectId.GenerateNewId(),
            ResourceId = id,
            ResourceType = ResourceType.ArtifactOnly,
            Kind = ResourceKind.Note,
            Source = "Manual",
            CreatedUtc = now,
            Title = title,
            Artifact = new ArtifactBody("Hello, Mongo!", "en", 1, now)
        };

        await Resources.InsertAsync(res);
        return id;
    }

    public void Dispose() { /* nothing: MongoClient is managed internally */ }
}
