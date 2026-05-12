namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public enum ResourceType { ArtifactOnly = 0, AssetOnly = 1 }
public enum ResourceKind { Note = 0, WebClip = 1 }
public enum RevisionKind { Edit = 0, Promote = 1 }
public enum ActKind { Commit = 0, Tentative = 1, Supersede = 2, Reconsider = 3, Endorse = 4 }

public sealed record ResourceLink(string Type, long ToResourceId);

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
    public List<ResourceLink> Links { get; init; } = new();

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


public class ResourceStore
{
    private readonly IMongoCollection<Resource> resources = null!;
    private readonly IMongoCollection<BsonDocument> counters = null!;

    protected ResourceStore() { }

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

    public virtual Task<Resource?> GetAsync(long id)
        => resources.Find(r => r.ResourceId == id).FirstOrDefaultAsync();

    public virtual async Task<long> AppendRevisionAsync(
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

    public virtual async Task<string> AppendActAsync(
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

    public virtual async Task<long?> SupersedeLatestCommitAsync(long resourceId, long currentRevisionId)
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


    /// <summary>Text search (lexical). Returns recent first by CreatedUtc.</summary>
    public virtual async Task<List<Resource>> TextSearchAsync(string query, int take = 50)
    {
        if (string.IsNullOrWhiteSpace(query)) return new();

        var filter = Builders<Resource>.Filter.Text(query);
        var sort = Builders<Resource>.Sort.Descending(r => r.CreatedUtc);

        var rows = await resources
            .Find(filter)
            .Sort(sort)
            .Limit(take)
            .ToListAsync();

        return rows;
    }

    /// <summary>Find by exact Title (case-insensitive). Returns null if not found.</summary>
    public Task<Resource?> FindByTitleAsync(string title)
    {
        var filter = Builders<Resource>.Filter.Where(r => r.Title != null && r.Title.ToLower() == title.ToLower());
        return resources.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>Create a titled Resource (Note) with empty body; returns new ResourceId.</summary>
    public async Task<long> CreateTitledResourceAsync(string title, string source = "Weaver", string? language = "en")
    {
        var id = await NextIdAsync();
        var now = DateTime.UtcNow;

        var res = new Resource
        {
            Id = ObjectId.GenerateNewId(),
            ResourceId = id,
            ResourceType = ResourceType.ArtifactOnly,
            Kind = ResourceKind.Note,
            Source = source,
            CreatedUtc = now,
            Title = title,
            Artifact = new ArtifactBody("", language, 1, now),
            Revisions = new(),
            Acts = new(),
            Tags = new(),
            Links = new()
        };

        await InsertAsync(res);
        return id;
    }

    /// <summary>Resolve a title; if missing and createIfMissing=true, create it.</summary>
    public virtual async Task<long?> ResolveOrCreateByTitleAsync(string title, bool createIfMissing)
    {
        var existing = await FindByTitleAsync(title);
        if (existing != null) return existing.ResourceId;

        if (!createIfMissing) return null;

        return await CreateTitledResourceAsync(title);
    }

    /// <summary>Adds an outgoing link if not already present.</summary>
    public virtual async Task UpsertLinkAsync(long fromResourceId, string linkType, long toResourceId)
    {
        var filter = Builders<Resource>.Filter.Eq(r => r.ResourceId, fromResourceId);

        // Prevent duplicates: check if an identical link exists
        var doc = await resources.Find(filter).FirstOrDefaultAsync();
        if (doc == null) return;

        if (doc.Links.Any(l => l.Type == linkType && l.ToResourceId == toResourceId)) return;

        var update = Builders<Resource>.Update.Push(r => r.Links, new ResourceLink(linkType, toResourceId));
        var res = await resources.UpdateOneAsync(filter, update);

        if (res.MatchedCount == 0)
            Console.WriteLine($"[UpsertLink] WARN: from={fromResourceId} not matched.");
    }


}
