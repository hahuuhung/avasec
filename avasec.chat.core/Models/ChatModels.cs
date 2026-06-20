using System;
using System.Collections.Generic;

namespace AVASec.Chat.Core.Models
{
    /// <summary>
    /// Chat Message model / Mô hình Tin nhắn Chat
    /// </summary>
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsBot { get; set; }
        public bool IsAgent { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
    }

    /// <summary>
    /// Message type / Loại tin nhắn
    /// </summary>
    public enum MessageType
    {
        Text,
        Image,
        File,
        System
    }

    /// <summary>
    /// Chat Session model / Mô hình Phiên Chat
    /// </summary>
    public class ChatSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ChatMessage> Messages { get; set; } = new();
        public string? AssignedAgentId { get; set; }
    }

    /// <summary>
    /// Support Agent model / Mô hình Nhân viên Hỗ trợ
    /// </summary>
    public class SupportAgent
    {
        public string AgentId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public int ActiveSessions { get; set; }
        public AgentStatus Status { get; set; } = AgentStatus.Available;
    }

    public enum AgentStatus
    {
        Available,
        Busy,
        Away,
        Offline
    }
}
