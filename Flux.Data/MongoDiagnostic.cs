namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Driver;

public enum MongoStatus { Ok, Unauthorized, Error }

public sealed record MongoDiagnosticResult(MongoStatus Status, string? Message = null);

public interface IMongoDiagnostic
{
    Task<MongoDiagnosticResult> CheckAsync();
}

public class MongoDiagnostic : IMongoDiagnostic
{
    private readonly IMongoDatabase _db;

    public MongoDiagnostic(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task<MongoDiagnosticResult> CheckAsync()
    {
        try
        {
            var cmd = new JsonCommand<object>("{ ping: 1 }");
            await _db.RunCommandAsync(cmd);

            var collections = await _db.ListCollectionNamesAsync();
            await collections.MoveNextAsync();

            var counter = _db.GetCollection<BsonDocument>("diagnostic");
            await counter.InsertOneAsync(new BsonDocument("ts", DateTime.UtcNow));

            return new MongoDiagnosticResult(MongoStatus.Ok);
        }
        catch (MongoCommandException mce) when (mce.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return new MongoDiagnosticResult(MongoStatus.Unauthorized, mce.Message);
        }
        catch (Exception ex)
        {
            return new MongoDiagnosticResult(MongoStatus.Error, ex.Message);
        }
    }
}
