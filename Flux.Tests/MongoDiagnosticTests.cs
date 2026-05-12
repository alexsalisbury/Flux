namespace Flux.Tests;

using Flux.Data;
using NSubstitute;

public class MongoDiagnosticTests
{
    // --- MongoDiagnosticResult ---

    [Fact]
    public void Result_Ok_HasNullMessage()
    {
        var result = new MongoDiagnosticResult(MongoStatus.Ok);
        Assert.Equal(MongoStatus.Ok, result.Status);
        Assert.Null(result.Message);
    }

    [Fact]
    public void Result_Error_CarriesMessage()
    {
        var result = new MongoDiagnosticResult(MongoStatus.Error, "connection refused");
        Assert.Equal(MongoStatus.Error, result.Status);
        Assert.Equal("connection refused", result.Message);
    }

    [Fact]
    public void Result_Unauthorized_CarriesMessage()
    {
        var result = new MongoDiagnosticResult(MongoStatus.Unauthorized, "not authorized");
        Assert.Equal(MongoStatus.Unauthorized, result.Status);
        Assert.Equal("not authorized", result.Message);
    }

    [Fact]
    public void Result_RecordEquality()
    {
        var a = new MongoDiagnosticResult(MongoStatus.Ok);
        var b = new MongoDiagnosticResult(MongoStatus.Ok);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Result_RecordInequality_DifferentStatus()
    {
        var a = new MongoDiagnosticResult(MongoStatus.Ok);
        var b = new MongoDiagnosticResult(MongoStatus.Error);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Result_RecordInequality_DifferentMessage()
    {
        var a = new MongoDiagnosticResult(MongoStatus.Error, "msg1");
        var b = new MongoDiagnosticResult(MongoStatus.Error, "msg2");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Result_WithExpression_ChangesStatus()
    {
        var ok = new MongoDiagnosticResult(MongoStatus.Ok);
        var err = ok with { Status = MongoStatus.Error, Message = "failed" };
        Assert.Equal(MongoStatus.Error, err.Status);
        Assert.Equal("failed", err.Message);
        Assert.Equal(MongoStatus.Ok, ok.Status);
    }

    // --- MongoStatus enum ---

    [Theory]
    [InlineData(MongoStatus.Ok, 0)]
    [InlineData(MongoStatus.Unauthorized, 1)]
    [InlineData(MongoStatus.Error, 2)]
    public void MongoStatus_HasExpectedValues(MongoStatus status, int expected)
    {
        Assert.Equal(expected, (int)status);
    }

    // --- IMongoDiagnostic contract ---

    [Fact]
    public async Task IMongoDiagnostic_CanBeMocked()
    {
        var mock = Substitute.For<IMongoDiagnostic>();
        mock.CheckAsync().Returns(Task.FromResult(new MongoDiagnosticResult(MongoStatus.Ok)));

        var result = await mock.CheckAsync();

        Assert.Equal(MongoStatus.Ok, result.Status);
    }

    [Fact]
    public async Task IMongoDiagnostic_MockReturnsError()
    {
        var mock = Substitute.For<IMongoDiagnostic>();
        mock.CheckAsync().Returns(Task.FromResult(new MongoDiagnosticResult(MongoStatus.Error, "timeout")));

        var result = await mock.CheckAsync();

        Assert.Equal(MongoStatus.Error, result.Status);
        Assert.Equal("timeout", result.Message);
    }

    [Fact]
    public async Task IMongoDiagnostic_MockReturnsUnauthorized()
    {
        var mock = Substitute.For<IMongoDiagnostic>();
        mock.CheckAsync().Returns(Task.FromResult(new MongoDiagnosticResult(MongoStatus.Unauthorized, "bad creds")));

        var result = await mock.CheckAsync();

        Assert.Equal(MongoStatus.Unauthorized, result.Status);
        Assert.Equal("bad creds", result.Message);
    }
}
