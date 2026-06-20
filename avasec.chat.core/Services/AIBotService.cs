using System;
using System.Threading.Tasks;
using AVASec.Chat.Core.Interfaces;

namespace AVASec.Chat.Core.Services
{
    /// <summary>
    /// AI Bot Service - Provides automated responses
    /// Dịch vụ Bot AI - Cung cấp phản hồi tự động
    /// </summary>
    public class AIBotService : IAIBotService
    {
        private readonly string[] _commonQuestions = new[]
        {
            "disk", "clean", "cleanup", "ram", "memory", "cpu", "virus", "scan",
            "dọn", "quét", "bộ nhớ", "đĩa", "tối ưu"
        };

        public bool CanHandleQuery(string message)
        {
            var lowerMessage = message.ToLowerInvariant();
            
            foreach (var keyword in _commonQuestions)
            {
                if (lowerMessage.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<string> GetResponseAsync(string userMessage, string context)
        {
            var lowerMessage = userMessage.ToLowerInvariant();

            // Disk cleanup questions / Câu hỏi về dọn dẹp đĩa
            if (lowerMessage.Contains("disk") || lowerMessage.Contains("clean") || lowerMessage.Contains("dọn"))
            {
                return "🧹 **Disk Cleanup / Dọn dẹp Đĩa**\n\n" +
                       "To clean your disk:\n" +
                       "1. Click the **Disk Cleanup** button on the Dashboard\n" +
                       "2. Select file categories to clean\n" +
                       "3. Click **Clean Now**\n\n" +
                       "Để dọn dẹp đĩa:\n" +
                       "1. Nhấp nút **Disk Cleanup** trên Dashboard\n" +
                       "2. Chọn các loại file cần dọn\n" +
                       "3. Nhấp **Clean Now**";
            }

            // RAM optimization / Tối ưu RAM
            if (lowerMessage.Contains("ram") || lowerMessage.Contains("memory") || lowerMessage.Contains("bộ nhớ"))
            {
                return "⚡ **RAM Optimization / Tối ưu RAM**\n\n" +
                       "To optimize RAM:\n" +
                       "1. Click **RAM Optimize** button\n" +
                       "2. Review running processes\n" +
                       "3. Close unnecessary applications\n\n" +
                       "Để tối ưu RAM:\n" +
                       "1. Nhấp nút **RAM Optimize**\n" +
                       "2. Xem các tiến trình đang chạy\n" +
                       "3. Đóng các ứng dụng không cần thiết";
            }

            // Virus scan / Quét virus
            if (lowerMessage.Contains("virus") || lowerMessage.Contains("scan") || lowerMessage.Contains("quét"))
            {
                return "🛡️ **Virus Scanner / Quét Virus**\n\n" +
                       "To scan for threats:\n" +
                       "1. Click **Quick Scan** for fast scan\n" +
                       "2. Or go to Virus Scanner for full scan\n" +
                       "3. Review and quarantine threats\n\n" +
                       "Để quét mối đe dọa:\n" +
                       "1. Nhấp **Quick Scan** để quét nhanh\n" +
                       "2. Hoặc vào Virus Scanner để quét toàn bộ\n" +
                       "3. Xem và cách ly các mối đe dọa";
            }

            // CPU usage / Sử dụng CPU
            if (lowerMessage.Contains("cpu"))
            {
                return "📊 **CPU Usage / Sử dụng CPU**\n\n" +
                       "High CPU usage can be caused by:\n" +
                       "- Too many programs running\n" +
                       "- Background processes\n" +
                       "- Malware\n\n" +
                       "Try using **RAM Optimize** to close unnecessary processes.\n\n" +
                       "Sử dụng CPU cao có thể do:\n" +
                       "- Quá nhiều chương trình chạy\n" +
                       "- Tiến trình nền\n" +
                       "- Phần mềm độc hại\n\n" +
                       "Thử dùng **RAM Optimize** để đóng các tiến trình không cần thiết.";
            }

            // Password Security / Bảo mật Mật khẩu
            if (lowerMessage.Contains("password") || lowerMessage.Contains("mật khẩu") || lowerMessage.Contains("authen"))
            {
                return "🔐 **Password Security / Bảo mật Mật khẩu**\n\n" +
                       "Tips for strong passwords:\n" +
                       "- Use at least 12 characters\n" +
                       "- Mix uppercase, lowercase, numbers, symbols\n" +
                       "- Use 2FA (Two-factor Authentication)\n" +
                       "- Do not reuse passwords\n\n" +
                       "Mẹo cho mật khẩu mạnh:\n" +
                       "- Dùng ít nhất 12 ký tự\n" +
                       "- Kết hợp chữ hoa, thường, số, ký tự đặc biệt\n" +
                       "- Sử dụng xác thực 2 bước (2FA)\n" +
                       "- Không dùng lại mật khẩu cũ";
            }

            // Firewall / Tường lửa
            if (lowerMessage.Contains("firewall") || lowerMessage.Contains("tường lửa") || lowerMessage.Contains("network"))
            {
                return "🧱 **Firewall Protection / Bảo vệ Tường lửa**\n\n" +
                       "The Firewall protects you from unauthorized access.\n" +
                       "- Keep Firewall ON always\n" +
                       "- Monitor blocked connections\n" +
                       "- Allow only trusted apps\n\n" +
                       "Tường lửa bảo vệ bạn khỏi truy cập trái phép.\n" +
                       "- Luôn BẬT Tường lửa\n" +
                       "- Theo dõi các kết nối bị chặn\n" +
                       "- Chỉ cho phép ứng dụng tin cậy";
            }

            // Ransomware / Mã độc tống tiền
            if (lowerMessage.Contains("ransom") || lowerMessage.Contains("mã hóa") || lowerMessage.Contains("lock"))
            {
                return "💀 **Ransomware / Mã độc tống tiền**\n\n" +
                       "Ransomware encrypts your files and demands payment.\n" +
                       "Action Plan:\n" +
                       "1. Disconnect internet immediately\n" +
                       "2. Do NOT pay the ransom\n" +
                       "3. Run **Full Scan** with Virus Scanner\n" +
                       "4. Restore files from backup\n\n" +
                       "Kế hoạch hành động:\n" +
                       "1. Ngắt internet ngay lập tức\n" +
                       "2. KHÔNG trả tiền chuộc\n" +
                       "3. Chạy **Full Scan** với Virus Scanner\n" +
                       "4. Khôi phục file từ bản sao lưu";
            }

            // Phishing / Lừa đảo
            if (lowerMessage.Contains("phishing") || lowerMessage.Contains("scam") || lowerMessage.Contains("lừa đảo") || lowerMessage.Contains("email"))
            {
                return "🎣 **Phishing Awareness / Cảnh giác Lừa đảo**\n\n" +
                       "Phishing attempts often come via Email or SMS.\n" +
                       "- Check the sender's email address carefully\n" +
                       "- Do NOT click suspicious links\n" +
                       "- Verify URLs before entering passwords\n\n" +
                       "Lừa đảo thường đến qua Email hoặc SMS.\n" +
                       "- Kiểm tra kỹ địa chỉ email người gửi\n" +
                       "- KHÔNG nhấp link đáng ngờ\n" +
                       "- Xác minh URL trước khi nhập mật khẩu";
            }

            // Updates / Cập nhật
            if (lowerMessage.Contains("update") || lowerMessage.Contains("cập nhật") || lowerMessage.Contains("patch"))
            {
                return "🔄 **System Updates / Cập nhật Hệ thống**\n\n" +
                       "Keeping software updated is critical for security.\n" +
                       "- Check Windows Updates regularly\n" +
                       "- Update AVA Security database\n" +
                       "- Update browsers and drivers\n\n" +
                       "Cập nhật phần mềm rất quan trọng cho bảo mật.\n" +
                       "- Kiểm tra Windows Update thường xuyên\n" +
                       "- Cập nhật cơ sở dữ liệu AVA Security\n" +
                       "- Cập nhật trình duyệt và driver";
            }

            // Default response / Phản hồi mặc định
            return "🤖 I'm here to help with AVA Security features! Try asking about:\n" +
                   "- Disk cleanup\n" +
                   "- RAM optimization\n" +
                   "- Virus scanning\n" +
                   "- CPU usage\n\n" +
                   "Or type **'support'** to connect with a human agent.\n\n" +
                   "---\n\n" +
                   "Tôi ở đây để giúp về các tính năng AVA Security! Hỏi về:\n" +
                   "- Dọn dẹp đĩa\n" +
                   "- Tối ưu RAM\n" +
                   "- Quét virus\n" +
                   "- Sử dụng CPU\n\n" +
                   "Hoặc gõ **'support'** để kết nối với nhân viên hỗ trợ.";
        }
    }
}
