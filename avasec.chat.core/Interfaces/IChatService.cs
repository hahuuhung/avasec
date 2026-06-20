using System.Threading.Tasks;
using AVASec.Chat.Core.Models;

namespace AVASec.Chat.Core.Interfaces
{
    /// <summary>
    /// Chat Service interface / Interface Dịch vụ Chat
    /// </summary>
    public interface IChatService
    {
        Task<ChatSession> StartSessionAsync(string userId, string userName);
        Task<ChatMessage> SendMessageAsync(string sessionId, string userId, string message);
        Task<ChatSession?> GetSessionAsync(string sessionId);
        Task EndSessionAsync(string sessionId);
        
        /// <summary>
        /// Event triggered when a new message is received / Sự kiện được kích hoạt khi nhận tin nhắn mới
        /// </summary>
        event Action<ChatMessage> MessageReceived;
    }

    /// <summary>
    /// AI Bot Service interface / Interface Dịch vụ Bot AI
    /// </summary>
    public interface IAIBotService
    {
        Task<string> GetResponseAsync(string userMessage, string context);
        bool CanHandleQuery(string message);
    }
}
