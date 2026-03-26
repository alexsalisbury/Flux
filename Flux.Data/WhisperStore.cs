namespace Flux.Data;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;


public enum WhisperChannel { STT = 0, OCR = 1, PDF = 2, Other = 3 }

public sealed record Whisper
{
    [BsonId] public ObjectId Id { get; init; }
    public long ResourceId { get; init; }
    public WhisperChannel Channel { get; init; }
    public string Text { get; init; } = "";
    public DateTime CreatedUtc { get; init; }
}

public sealed class WhisperStore
{
    private readonly IMongoCollection<Whisper> whispers;

    public WhisperStore(MongoCollections col) => whispers = col.Whispers;

    public Task InsertAsync(Whisper w) => whispers.InsertOneAsync(w);
}