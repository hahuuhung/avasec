using System.Windows;
using AVASec.UI.Controls;

namespace AVASec.UI.Views
{
    public partial class ChatWindow : Window
    {
        public ChatWindow(ChatWidget chatWidget)
        {
            InitializeComponent();
            
            // Re-use current ChatWidget / Sử dụng lại ChatWidget hiện tại
            MainContainer.Child = chatWidget;
            
            // Header is draggable / Phần đầu có thể kéo được
            chatWidget.CloseRequested += (s, e) => this.Close();
        }
    }
}
