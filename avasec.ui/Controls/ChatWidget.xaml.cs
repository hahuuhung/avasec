using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using AVASec.Chat.Core.Models;
using AVASec.Chat.Core.Interfaces;
using System;

namespace AVASec.UI.Controls
{
    /// <summary>
    /// Chat Widget - Real-time support chat
    /// Widget Chat - Chat hỗ trợ thời gian thực
    /// </summary>
    public partial class ChatWidget : System.Windows.Controls.UserControl
    {
        private readonly IChatService _chatService;
        private ChatSession? _currentSession;
        private ObservableCollection<ChatMessage> Messages { get; set; }

        public ChatWidget(IChatService chatService)
        {
            InitializeComponent();
            _chatService = chatService;
            Messages = new ObservableCollection<ChatMessage>();
            MessagesContainer.ItemsSource = Messages;
            
            // Subscribe to events / Đăng ký sự kiện
            _chatService.MessageReceived += OnMessageReceived;
            this.Unloaded += ChatWidget_Unloaded;

            InitializeChatAsync();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.DragMove();
                }
            }
        }

        private void ChatWidget_Unloaded(object sender, RoutedEventArgs e)
        {
            _chatService.MessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(ChatMessage message)
        {
            // Update UI on main thread / Cập nhật UI trên luồng chính
            Dispatcher.Invoke(() =>
            {
                if (!Messages.Contains(message))
                {
                    Messages.Add(message);
                    ScrollToBottom();
                }
            });
        }

        private async void InitializeChatAsync()
        {
            try
            {
                // Start chat session / Bắt đầu phiên chat
                // Auto-generate ID for Guest mode / Tự động tạo ID cho chế độ Guest
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var userId = $"Guest-{uniqueId}";
                var userName = $"Guest User {uniqueId}";
                
                _currentSession = await _chatService.StartSessionAsync(userId, userName);
                
                // Add welcome messages / Thêm tin nhắn chào mừng
                foreach (var message in _currentSession.Messages)
                {
                    if (!Messages.Contains(message))
                        Messages.Add(message);
                }
                
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessageAsync();
                e.Handled = true;
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync()
        {
            var messageText = MessageInput.Text?.Trim();
            
            if (string.IsNullOrEmpty(messageText) || _currentSession == null)
                return;

            try
            {
                // Clear input / Xóa input
                MessageInput.Text = string.Empty;
                
                // Send message / Gửi tin nhắn
                var userMessage = await _chatService.SendMessageAsync(
                    _currentSession.SessionId,
                    Environment.UserName,
                    messageText
                );
                
                // Add user message to UI immediately / Thêm tin nhắn người dùng vào UI ngay lập tức
                Messages.Add(userMessage);

                // Bot/Agent response will come via OnMessageReceived event
                // Phản hồi Bot/Agent sẽ đến qua sự kiện OnMessageReceived
                
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler? MinimizeRequested;
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
             var window = Window.GetWindow(this);
             if (window != null)
             {
                 window.WindowState = WindowState.Minimized;
             }
             else
             {
                 MinimizeRequested?.Invoke(this, EventArgs.Empty);
             }
        }
        
        public event EventHandler? CloseRequested;
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ScrollToBottom()
        {
            MessagesScrollViewer.ScrollToEnd();
        }
    }
}
