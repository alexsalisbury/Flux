namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public sealed class AssetStore
{
    private readonly IMongoCollection<Asset> assets;

    public AssetStore(MongoCollections col) => assets = col.Assets;

    public Task InsertAsync(Asset a) => assets.InsertOneAsync(a);
}

public sealed record Asset
{
    [BsonId] public ObjectId Id { get; init; }
    public long ResourceId { get; init; }
    public string ContentType { get; init; } = "";
    public string StoragePath { get; init; } = "";
}