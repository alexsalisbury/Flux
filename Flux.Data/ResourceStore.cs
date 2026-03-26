namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public enum ResourceType { ArtifactOnly = 0, AssetOnly = 1 }
public enum ResourceKind { Note = 0, WebClip = 1 }
public enum RevisionKind { Edit = 0, Promote = 1 }
public enum ActKind { Commit = 0, Tentative = 1 }

public sealed record ArtifactBody(
    string Body,
    string? Language,
    int Version,
    DateTime EditedUtc
);

public sealed record Resource
{
    [BsonId]
    public ObjectId Id { get; init; }

    public long ResourceId { get; init; }
    public ResourceType ResourceType { get; init; }
    public ResourceKind Kind { get; init; }
    public string Source { get; init; } = "Manual";
    public DateTime CreatedUtc { get; init; }
    public string? Title { get; init; }

    public ArtifactBody? Artifact { get; init; }
    public List<string> Tags { get; init; } = new();
}


public sealed class ResourceStore
{
    private readonly IMongoCollection<Resource> resources;
    private readonly IMongoCollection<BsonDocument> counters;

    public ResourceStore(MongoCollections col)
    {
        resources = col.Resources;
        counters = col.Database.GetCollection<BsonDocument>("counters");
    }

    public async Task<long> NextIdAsync()
    {
        var doc = await counters.FindOneAndUpdateAsync(
            Builders<BsonDocument>.Filter.Eq("_id", "resId"),
            Builders<BsonDocument>.Update.Inc("value", 1),
            new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            });

        return doc["value"].ToInt64();
    }

    public Task InsertAsync(Resource res) => resources.InsertOneAsync(res);

    public Task<Resource?> GetAsync(long id)
        => resources.Find(r => r.ResourceId == id).FirstOrDefaultAsync();
}
