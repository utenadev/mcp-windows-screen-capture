using System.Text.Json;
using Xunit;

namespace WindowsScreenCapture.Tests;

public class McpSessionTests
{
    [Fact]
    public void McpSession_CanBeCreated()
    {
        var session = new McpSession();
        
        Assert.NotNull(session.Id);
        Assert.NotEmpty(session.Id);
        Assert.NotNull(session.MessageChannel);
        Assert.False(session.IsInitialized);
        Assert.True(session.CreatedAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public void McpSession_Touch_UpdatesLastActivity()
    {
        var session = new McpSession();
        var initialActivity = session.LastActivity;
        
        Thread.Sleep(10); // Small delay
        session.Touch();
        
        Assert.True(session.LastActivity > initialActivity);
    }
}

public class McpSessionManagerTests
{
    [Fact]
    public void CreateSession_ReturnsValidSession()
    {
        var manager = new McpSessionManager();
        
        var session = manager.CreateSession();
        
        Assert.NotNull(session);
        Assert.NotEmpty(session.Id);
    }
    
    [Fact]
    public void TryGetSession_ExistingSession_ReturnsTrue()
    {
        var manager = new McpSessionManager();
        var session = manager.CreateSession();
        
        var result = manager.TryGetSession(session.Id, out var retrievedSession);
        
        Assert.True(result);
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.Id, retrievedSession!.Id);
    }
    
    [Fact]
    public void TryGetSession_NonExistingSession_ReturnsFalse()
    {
        var manager = new McpSessionManager();
        
        var result = manager.TryGetSession("non-existing-id", out var session);
        
        Assert.False(result);
        Assert.Null(session);
    }
    
    [Fact]
    public void RemoveSession_ExistingSession_RemovesSuccessfully()
    {
        var manager = new McpSessionManager();
        var session = manager.CreateSession();
        
        manager.RemoveSession(session.Id);
        
        var result = manager.TryGetSession(session.Id, out _);
        Assert.False(result);
    }
}
