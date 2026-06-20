using AVASec.Authentication.Services;
using AVASec.Core.Interfaces;
using AVASec.Database;
using AVASec.UI.Services;
using System.Windows;
using System.Windows.Controls;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Login Window / Cửa sổ đăng nhập
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthenticationService _authService;
        private readonly AVASecContext _context;

        public LoginWindow()
        {
            InitializeComponent();
            _context = new AVASecContext();
            _authService = new HybridAuthenticationService(_context);
            
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize Language Selection
            var currentLang = LanguageService.Instance.CurrentLanguage;
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == currentLang)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Use login tab controls / Sử dụng điều khiển tab đăng nhập
            string username = LoginUsernameTextBox.Text;
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password. / Vui lòng nhập tên đăng nhập và mật khẩu.", 
                    "Validation Error / Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _authService.LoginAsync(username, password);

            if (result.Success && result.UserId.HasValue)
            {
                MessageBox.Show(result.Message, "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Return success result to parent window / Trả về kết quả thành công cho cửa sổ cha
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Login Failed / Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Use register tab controls / Sử dụng điều khiển tab đăng ký
            string username = RegisterUsernameTextBox.Text;
            string password = RegisterPasswordBox.Password;
            string email = RegisterEmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("All fields are required. / Tất cả các trường là bắt buộc.", 
                    "Validation Error / Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _authService.RegisterAsync(username, password, email);

            if (result.Success)
            {
                MessageBox.Show(result.Message + "\n\nYou will receive a 14-day trial license!\nBạn sẽ nhận giấy phép dùng thử 14 ngày!", 
                    "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear register form / Xóa form đăng ký
                RegisterUsernameTextBox.Clear();
                RegisterPasswordBox.Clear();
                RegisterEmailTextBox.Clear();
            }
            else
            {
                MessageBox.Show(result.Message, "Registration Failed / Đăng ký thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // Open URL in default browser / Mở URL trong trình duyệt mặc định
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void LoginUsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void QuickRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open Quick Register window / Mở cửa sổ đăng ký nhanh
            var quickRegisterWindow = new QuickRegisterWindow();
            quickRegisterWindow.ShowDialog();
        }

        // Window Controls
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                LanguageService.Instance.SetLanguage(item.Tag.ToString());
            }
        }
    }
}
