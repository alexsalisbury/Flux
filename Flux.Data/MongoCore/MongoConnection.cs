namespace Flux.Data.MongoCore;

using MongoDB.Driver;

public sealed class MongoConnection
{
    public IMongoDatabase Database { get; }

    public MongoConnection(string conn, string dbName)
    {
        var settings = MongoClientSettings.FromConnectionString(conn);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        var client = new MongoClient(settings);
        Database = client.GetDatabase(dbName);
    }
}