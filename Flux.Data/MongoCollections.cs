namespace Flux.Data;

using MongoDB.Driver;

public sealed class MongoCollections
{
    public IMongoCollection<Resource> Resources { get; }
    public IMongoCollection<Asset> Assets { get; }
    public IMongoCollection<Whisper> Whispers { get; }

    public IMongoDatabase Database { get; }

    public MongoCollections(IMongoDatabase db)
    {
        Database = db;
        Resources = db.GetCollection<Resource>("resources");
        Assets = db.GetCollection<Asset>("assets");
        Whispers = db.GetCollection<Whisper>("whispers");
    }
}
