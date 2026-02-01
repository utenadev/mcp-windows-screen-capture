using System.Text.Json;
using System.Threading.Channels;

public class McpSession
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public Channel<string> MessageChannel { get; } = Channel.CreateUnbounded<string>();
    public bool IsInitialized { get; set; } = false;
    public string ProtocolVersion { get; set; } = "2024-11-05";
    
    public void Touch()
    {
        LastActivity = DateTime.UtcNow;
    }
}

public class McpSessionManager
{
    private readonly Dictionary<string, McpSession> _sessions = new();
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(1);
    
    public McpSession CreateSession()
    {
        var session = new McpSession();
        _sessions[session.Id] = session;
        Console.WriteLine($"[MCP Session] Created: {session.Id}");
        return session;
    }
    
    public bool TryGetSession(string sessionId, out McpSession? session)
    {
        if (_sessions.TryGetValue(sessionId, out session))
        {
            if (session != null && DateTime.UtcNow - session.LastActivity < _sessionTimeout)
            {
                session.Touch();
                return true;
            }
            _sessions.Remove(sessionId);
        }
        session = null;
        return false;
    }
    
    public void RemoveSession(string sessionId)
    {
        if (_sessions.Remove(sessionId))
        {
            Console.WriteLine($"[MCP Session] Removed: {sessionId}");
        }
    }
    
    public void CleanupExpiredSessions()
    {
        var expired = _sessions
            .Where(s => DateTime.UtcNow - s.Value.LastActivity >= _sessionTimeout)
            .Select(s => s.Key)
            .ToList();
            
        foreach (var sessionId in expired)
        {
            _sessions.Remove(sessionId);
            Console.WriteLine($"[MCP Session] Expired: {sessionId}");
        }
    }
}
