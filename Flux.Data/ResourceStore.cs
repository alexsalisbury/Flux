namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public enum ResourceType { ArtifactOnly = 0, AssetOnly = 1 }
public enum ResourceKind { Note = 0, WebClip = 1 }
public enum RevisionKind { Edit = 0, Promote = 1 }
public enum ActKind { Commit = 0, Tentative = 1, Supersede = 2 }


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

    public List<Revision> Revisions { get; init; } = new();
    public List<Act> Acts { get; init; } = new();

    public List<string> Tags { get; init; } = new();
}

public sealed record Revision(
    long RevisionId,
    RevisionKind Kind,
    string Body,
    DateTime CreatedUtc,
    long? BasedOnRevisionId
);

public sealed record Act(
    string ActId,
    long RevisionId,
    ActKind Kind,
    DateTime CreatedUtc,
    string? Note
);


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

    public async Task<long> AppendRevisionAsync(
    long resourceId,
    RevisionKind kind,
    string body,
    long? basedOn = null)
    {
        var filter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId);

        // Get current max revision number
        var projection = Builders<Resource>.Projection.Expression(
            r => r.Revisions.Count == 0 ? 0 : r.Revisions.Max(x => x.RevisionId));
        var currentMax = await resources.Find(filter).Project(projection).FirstOrDefaultAsync();

        var next = currentMax + 1;

        var revision = new Revision(
            RevisionId: next,
            Kind: kind,
            Body: body,
            CreatedUtc: DateTime.UtcNow,
            BasedOnRevisionId: basedOn
        );

        // Update snapshot Artifact when saving OR committing
        var update = Builders<Resource>.Update
            .Push(r => r.Revisions, revision)
            .Set(r => r.Artifact, new ArtifactBody(
                Body: body,
                Language: "en",
                Version: (int)next,
                EditedUtc: DateTime.UtcNow
            ));

        await resources.UpdateOneAsync(filter, update);
        return next;
    }

    public async Task<string> AppendActAsync(
        long resourceId,
        long revisionId,
        ActKind kind,
        string? note = null)
    {
        var actId = Guid.NewGuid().ToString("N");
        var act = new Act(
            ActId: actId,
            RevisionId: revisionId,
            Kind: kind,
            CreatedUtc: DateTime.UtcNow,
            Note: note);

        var filter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId);
        var update = Builders<Resource>.Update.Push(r => r.Acts, act);

        await resources.UpdateOneAsync(filter, update);
        return actId;
    }

    public async Task<long?> SupersedeLatestCommitAsync(long resourceId, long currentRevisionId)
    {
        var doc = await resources.Find(r => r.ResourceId == resourceId).FirstOrDefaultAsync();
        if (doc == null) return null;

        var prior = doc.Acts
            .Where(a => a.Kind == ActKind.Commit && a.RevisionId != currentRevisionId)
            .OrderByDescending(a => a.CreatedUtc)
            .FirstOrDefault();

        if (prior is null) return null;

        await AppendActAsync(resourceId, prior.RevisionId, ActKind.Supersede, "Auto-superseded");
        return prior.RevisionId;
    }
}
