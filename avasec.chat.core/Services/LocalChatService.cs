using System.Collections.Concurrent;
using AVASec.Chat.Core.Interfaces;
using AVASec.Chat.Core.Models;

namespace AVASec.Chat.Core.Services;

/// <summary>
/// Offline-first chat — local AI bot only, no server required.
/// </summary>
public class LocalChatService : IChatService
{
    private readonly IAIBotService _botService;
    private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

    public event Action<ChatMessage>? MessageReceived;

    public LocalChatService(IAIBotService botService)
    {
        _botService = botService;
    }

    public Task<ChatSession> StartSessionAsync(string userId, string userName)
    {
        var session = new ChatSession
        {
            UserId = userId,
            UserName = userName,
            StartTime = DateTime.UtcNow
        };
        _sessions[session.SessionId] = session;

        var welcome = new ChatMessage
        {
            UserId = "bot",
            UserName = "Vigil Assistant",
            Message = "Xin chào! Tôi là trợ lý Vigil — hoạt động 100% trên máy bạn.\nHello! I'm Vigil Assistant — fully offline on your PC.",
            IsBot = true,
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(welcome);

        return Task.FromResult(session);
    }

    public async Task<ChatMessage> SendMessageAsync(string sessionId, string userId, string message)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            session = await StartSessionAsync(userId, "User");
            sessionId = session.SessionId;
        }

        session.Messages.Add(new ChatMessage
        {
            UserId = userId,
            UserName = session.UserName,
            Message = message,
            Timestamp = DateTime.UtcNow
        });

        var reply = await _botService.GetResponseAsync(message, sessionId);
        var botMessage = new ChatMessage
        {
            UserId = "bot",
            UserName = "Vigil Assistant",
            Message = reply,
            IsBot = true,
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(botMessage);
        MessageReceived?.Invoke(botMessage);

        return botMessage;
    }

    public Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task EndSessionAsync(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }
}
