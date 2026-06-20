using System;
using System.Windows;
using System.Windows.Controls;
using AVASec.Authentication.Services;
using AVASec.Database;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Core.Interfaces;
using AVASec.UI.Services;

namespace AVASec.UI.Views
{
    public partial class LoginOverlay : System.Windows.Controls.UserControl
    {
        private readonly IAuthenticationService _authService;
        
        // Event for parent to subscribe to
        public event EventHandler<int> LoginSuccessful;

        public LoginOverlay()
        {
            InitializeComponent();
            
            // Resolve services from DI / Lấy dịch vụ từ DI
            _authService = App.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            Loaded += LoginOverlay_Loaded;
        }

        private void LoginOverlay_Loaded(object sender, RoutedEventArgs e)
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
            string username = LoginUsernameTextBox.Text;
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _authService.LoginAsync(username, password);

            if (result.Success && result.UserId.HasValue)
            {
                // Trigger event
                LoginSuccessful?.Invoke(this, result.UserId.Value);
            }
            else
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
             // Proceed as Guest (UserId = 0)
             LoginSuccessful?.Invoke(this, 0);
             this.Visibility = Visibility.Collapsed;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsernameTextBox.Text;
            string password = RegisterPasswordBox.Password;
            string email = RegisterEmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _authService.RegisterAsync(username, password, email);

            if (result.Success)
            {
                MessageBox.Show(result.Message + "\n\nYou can now login with your 14-day trial account.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RegisterUsernameTextBox.Clear();
                RegisterPasswordBox.Clear();
                RegisterEmailTextBox.Clear();
            }
            else
            {
                MessageBox.Show(result.Message, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                LanguageService.Instance.SetLanguage(item.Tag.ToString());
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Switch to Trial Mode / Chuyển sang chế độ dùng thử
            this.Visibility = Visibility.Collapsed;
        }
    }
}
