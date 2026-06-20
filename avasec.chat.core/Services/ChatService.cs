using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using AVASec.Chat.Core.Interfaces;
using AVASec.Chat.Core.Models;
using SocketIOClient;
using System.Text.Json;
using System.Collections.Specialized; // Added for NameValueCollection if needed, or just standard collections if I was wrong.
// But wait, NameValueCollection is in System.Collections.Specialized.

namespace AVASec.Chat.Core.Services
{
    /// <summary>
    /// Chat Service implementation / Triển khai Dịch vụ Chat
    /// Uses Socket.IO for real-time communication with Node.js Backend
    /// </summary>
    public class ChatService : IChatService, IDisposable
    {
        private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
        private readonly IAIBotService _botService;
        private SocketIO? _socket;
        private const string SERVER_URL = "http://localhost:3000";
        
        public event Action<ChatMessage>? MessageReceived;

        public ChatService(IAIBotService botService)
        {
            _botService = botService;
        }

        public async Task<ChatSession> StartSessionAsync(string userId, string userName)
        {
            var session = new ChatSession
            {
                UserId = userId,
                UserName = userName,
                StartTime = DateTime.UtcNow
            };

            _sessions[session.SessionId] = session;

            // Initialize Socket.IO connection
            try
            {
                // Fix: convert string to Uri
                var uri = new Uri(SERVER_URL);
                
                // Fix: Query requires correct type.
                // If it is NameValueCollection (based on previous error), we use it.
                // However, SocketIOClient 3.1+ uses Dictionary<string, string>.
                // Maybe the installed version is older or weird?
                // I will try to use Dictionary<string, string> again but verify the error description in my mind.
                // "Cannot implicitly convert ... to NameValueCollection" -> Target IS NameValueCollection.
                // So I will use NameValueCollection.
                
                /*
                var query = new NameValueCollection();
                query.Add("userId", userId);
                query.Add("username", userName);
                query.Add("role", "user");
                */
                // Wait, NameValueCollection initializer:
                /*
                var query = new NameValueCollection
                {
                    { "userId", userId },
                    { "username", userName },
                    { "role", "user" }
                };
                */
                // Wait, SocketIOClient.SocketIOOptions.Query might be Dictionary<string,string> in some versions.
                // If I saw the error, I must trust it.
                // BUT, System.Collections.Specialized.NameValueCollection is not serializable readily by System.Text.Json without converter?
                // SocketIOClient handles it.
                
                // Let's assume the property is IEnumerable<KeyValuePair<string, string>> used as Dictionary in recent versions.
                // The error was: 'Cannot implicitly convert type 'System.Collections.Generic.Dictionary<string, string>' to 'System.Collections.Specialized.NameValueCollection''.
                // So the property type IS NameValueCollection.
                
                var query = new System.Collections.Generic.Dictionary<string, string>
                {
                     { "userId", userId },
                     { "username", userName },
                     { "role", "user" }
                };
                
                // WAIT! If the property type is NameValueCollection, I can't assign Dictionary.
                // I must assign NameValueCollection. 
                // But SocketIOClient v3/v4 usually doesn't use NameValueCollection.
                // Maybe I am using a very old/new version or a wrapper?
                // PackageReference says 4.0.0.2.
                // Verify: SocketIOClient v3.0.0 breaking change: Query is Dictionary<string, string>.
                // Maybe the error message I saw was 'Cannot convert ... to ...' but I misread the target?
                // "Cannot implicitly convert type 'System.Collections.Generic.Dictionary<string, string>' to 'System.Collections.Specialized.NameValueCollection'"
                // This implies the TARGET (Left Hand Side) is NameValueCollection?
                // OR the SOURCE (Right Hand Side) is Dictionary and it tries to convert to NameValueCollection because the property type is NameValueCollection.
                
                // I will try to use NameValueCollection just to be safe if that's what it wants.
                // But NameValueCollection is not generic.
                
                /*
                var query = new NameValueCollection();
                query["userId"] = userId;
                query["username"] = userName;
                query["role"] = "user";
                */
                
                // ACTUALLY, checking NuGet for SocketIOClient 4.0.0.2... It seems Query is Dictionary<string, string>.
                // Why would the compiler complain about NameValueCollection?
                // Maybe I am importing another namespace that conflicts?
                // Or maybe I am initializing a different property?
                
                // Let's look at the error again.
                // D:\sysanti-UInew\sysanti-UInew\AVASec.Chat.Core\Services\ChatService.cs(46,29): error CS0029: Cannot implicitly convert type 'System.Collections.Generic.Dictionary<string, string>' to 'System.Collections.Specialized.NameValueCollection'
                
                // 46,29 is inside the object initializer: `Query = new Dictionary...`
                // This confirms `Query` property expects `NameValueCollection`.
                
                // Review: SocketIOClient.NET (by Dogu) vs SocketIOClient (by 1999)? 
                // The namespace `SocketIOClient` matches.
                // Maybe it is a different fork/version or I'm hallucinating the standard.
                // I will trust the compiler.
                
                // So I will use NameValueCollection.
                
                /*
                var options = new SocketIOOptions();
                options.Query = new NameValueCollection 
                {
                    { "userId", userId },
                    { "username", userName },
                    { "role", "user" }
                };
                */
                // Wait, NameValueCollection doesn't support collection initializer in all versions?
                // It does in C# 6+.
                
                // Also fix new SocketIO(uri).
                
                // So, replace the whole block properly.
                
                _socket = new SocketIO(uri, new SocketIOOptions
                {
                    // If Query needs to be NameValueCollection, I can't assume dictionary works.
                    // But I need to be careful about JSON serialization if the library expects it.
                    // I will use NameValueCollection since the compiler insisted.
                    // Or maybe try 'Query = new Dictionary<string, string> ... ' failed.
                    // I will leave it empty if I can't figure it out, but I need the query.
                    
                    // Let's try to pass the query string manually in the URI?
                    // new Uri(SERVER_URL + "?userId=...")
                    // That's a valid workaround if Query property is troublesome.
                    // But I prefer using options.
                    
                    // Let's try using NameValueCollection.
                    /*
                    Query = new NameValueCollection
                    {
                        { "userId", userId },
                        { "username", userName },
                        { "role", "user" }
                    }
                    */
                    // Need to verify if NameValueCollection is available. It is in System.Collections.Specialized.
                    
                    Reconnection = true
                });
                
                // But wait, if I use NameValueCollection, I need to instantiate it.
                // And I can't use collection initializer if it's not supported by the type (Add method).
                // NameValueCollection has Add(string, string). So it should work.

                // HOWEVER, to avoid "NameValueCollection" dependency if I can avoid it:
                // I will try constructing the query string manually and append to Uri.
                // That is safer and universal.
                // "http://localhost:3000/?userId=...&username=...&role=user"
                // SocketIO will handle the rest.
                
                // So: I will remove Query from options and put it in URI.
                
                // Wait, SocketIO constructor takes (string url, SocketIOOptions options).
                // The error said "cannot convert from string to Uri".
                // So I use new Uri(modifiedUrl).
                
                string queryStr = $"?userId={Uri.EscapeDataString(userId)}&username={Uri.EscapeDataString(userName)}&role=user";
                var fullUri = new Uri(SERVER_URL + queryStr);
                
                _socket = new SocketIO(fullUri, new SocketIOOptions
                {
                    Reconnection = true
                });
                
                // This avoids the Query type issue entirely.

                _socket.On("receive_message", async response =>
                {
                    try
                    {
                        var data = response.GetValue<JsonElement>(0);
                        string content = data.GetProperty("content").GetString() ?? "";
                        string from = "Admin"; 
                        if (data.TryGetProperty("from", out var fromProp))
                        {
                            from = fromProp.GetString() ?? "Admin";
                        }

                        // Determine if message is suitable for mapping
                        var chatMessage = new ChatMessage
                        {
                            UserId = "admin", // Represents support agent
                            UserName = from,
                            Message = content,
                            IsAgent = true,
                            Timestamp = DateTime.UtcNow
                        };

                        MessageReceived?.Invoke(chatMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing message: {ex.Message}");
                    }
                });

                _socket.OnConnected += async (sender, e) =>
                {
                    Console.WriteLine("Socket connected");
                    await Task.CompletedTask;
                };

                await _socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket connection error: {ex.Message}");
                // Fallback or just log, session is still created locally
            }

            // Send welcome message / Gửi tin nhắn chào mừng
            var welcomeMessage = new ChatMessage
            {
                UserId = "bot",
                UserName = "AVA Security Assistant",
                Message = "Xin chào! Tôi là trợ lý AVA Security. Tôi có thể giúp gì cho bạn? 👋\n\nHello! I'm AVA Security Assistant. How can I help you? 👋",
                IsBot = true,
                Timestamp = DateTime.UtcNow
            };

            session.Messages.Add(welcomeMessage);
            
            return session;
        }

        public async Task<ChatMessage> SendMessageAsync(string sessionId, string userId, string message)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                 // Create temporary session if not exists logic could go here, but for now throwing or creating new
                 throw new ArgumentException("Session not found");
            }

            var userMessage = new ChatMessage
            {
                UserId = userId,
                UserName = session.UserName,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            session.Messages.Add(userMessage);

            bool handled = false;

            // 1. Check Local AI Bot / Kiểm tra Bot AI cục bộ
            if (_botService.CanHandleQuery(message))
            {
                var response = await _botService.GetResponseAsync(message, sessionId);
                var botMessage = new ChatMessage
                {
                    UserId = "bot",
                    UserName = "AVA Security Assistant",
                    Message = response,
                    IsBot = true,
                    Timestamp = DateTime.UtcNow
                };

                session.Messages.Add(botMessage);
                MessageReceived?.Invoke(botMessage);
                handled = true;
            }
            
            // 2. Always send to Server for Admin to see logic
            // But with Socket.IO, we emit even if bot handled it? 
            // Maybe we want admin to see the conversation.
            
            if (_socket != null && _socket.Connected)
            {
                try
                {
                    await _socket.EmitAsync("send_message", new object[] { new
                    {
                        content = message
                    } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Socket emit error: {ex.Message}");
                }
            }

            return userMessage;
        }

        public async Task<ChatSession?> GetSessionAsync(string sessionId)
        {
             _sessions.TryGetValue(sessionId, out var session);
             return await Task.FromResult(session);
        }

        public async Task EndSessionAsync(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.IsActive = false;
                session.EndTime = DateTime.UtcNow;
            }
            
            if (_socket != null)
            {
                await _socket.DisconnectAsync();
                _socket.Dispose();
                _socket = null;
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}

