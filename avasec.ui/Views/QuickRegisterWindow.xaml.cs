using AVASec.Authentication.Services;
using AVASec.Core.Interfaces;
using AVASec.Database;
using AVASec.UI.Services;
using System.Windows;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Quick Register Window / Cửa sổ đăng ký nhanh
    /// For testing login functionality
    /// </summary>
    public partial class QuickRegisterWindow : Window
    {
        private readonly IAuthenticationService _authService;
        private readonly AVASecContext _context;

        public QuickRegisterWindow()
        {
            InitializeComponent();
            _context = new AVASecContext();
            _authService = new HybridAuthenticationService(_context);
        }

        private async void CreateTestAccount_Click(object sender, RoutedEventArgs e)
        {
            // Create test account / Tạo tài khoản test
            string username = "admin";
            string password = "admin123";
            string email = "admin@avasec.app";

            var result = await _authService.RegisterAsync(username, password, email);

            if (result.Success)
            {
                MessageBox.Show(
                    $"✅ Test account created!\n\n" +
                    $"Username: {username}\n" +
                    $"Password: {password}\n\n" +
                    $"Tài khoản test đã tạo!\n" +
                    $"Tên đăng nhập: {username}\n" +
                    $"Mật khẩu: {password}",
                    "Success / Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
