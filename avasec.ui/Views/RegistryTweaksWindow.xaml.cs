using AVASec.Optimization.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Registry Tweaks Window / Cửa sổ tối ưu Registry
    /// </summary>
    public partial class RegistryTweaksWindow : Window
    {
        private readonly RegistryTweaksService _tweaksService;

        public RegistryTweaksWindow()
        {
            InitializeComponent();
            _tweaksService = new RegistryTweaksService();
            LoadTweaks();
        }

        private void LoadTweaks()
        {
            var tweaks = _tweaksService.GetGeneralTweaks();
            
            // Group by category / Nhóm theo danh mục
            var categories = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<RegistryTweak>>();
            
            foreach (var tweak in tweaks)
            {
                if (!categories.ContainsKey(tweak.Category))
                {
                    categories[tweak.Category] = new System.Collections.Generic.List<RegistryTweak>();
                }
                categories[tweak.Category].Add(tweak);
            }

            // Create UI for each category / Tạo UI cho mỗi danh mục
            foreach (var category in categories)
            {
                // Category header / Tiêu đề danh mục
                var categoryHeader = new TextBlock
                {
                    Text = category.Key,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)Application.Current.Resources["AccentTealBrush"],
                    Margin = new Thickness(0, 20, 0, 10)
                };
                TweaksContainer.Children.Add(categoryHeader);

                // Add tweaks in this category / Thêm tweaks trong danh mục này
                foreach (var tweak in category.Value)
                {
                    var tweakCard = CreateTweakCard(tweak);
                    TweaksContainer.Children.Add(tweakCard);
                }
            }
        }

        // Window Controls
        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TrayButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                MessageBox.Show("Please use the main Dashboard for user login.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); 
            }
            catch {}
        }

        private void ChatToggle_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Support Chat is available on the main Dashboard.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
             try 
             {
                 if (Application.Current is App app && App.ServiceProvider != null)
                 {
                     var win = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<SettingsView>(App.ServiceProvider);
                     win.Owner = this;
                     win.ShowDialog();
                 }
                 else
                 {
                     new SettingsView().ShowDialog();
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error opening Settings: {ex.Message}");
             }
        }

        private void Toolbox_Click(object sender, RoutedEventArgs e)
        {
            try { new ToolboxWindow { Owner = this }.ShowDialog(); }
            catch {}
        }

        private Border CreateTweakCard(RegistryTweak tweak)
        {
            // Check current status / Kiểm tra trạng thái hiện tại
            bool isEnabled = _tweaksService.IsTweakEnabled(tweak);

            var card = new Border
            {
                Background = (Brush)Application.Current.Resources["DarkSurfaceBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = (Brush)Application.Current.Resources["DarkBorderBrush"],
                BorderThickness = new Thickness(1)
            };

            // Hover effect
            card.MouseEnter += (s, e) => card.BorderBrush = (Brush)Application.Current.Resources["AccentBlueBrush"];
            card.MouseLeave += (s, e) => card.BorderBrush = (Brush)Application.Current.Resources["DarkBorderBrush"];

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left side - Info / Bên trái - Thông tin
            var infoStack = new StackPanel();
            
            var nameViText = new TextBlock
            {
                Text = tweak.NameVi,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"]
            };
            infoStack.Children.Add(nameViText);

            var nameText = new TextBlock
            {
                Text = tweak.Name,
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                Margin = new Thickness(0, 2, 0, 6)
            };
            infoStack.Children.Add(nameText);

            var descViText = new TextBlock
            {
                Text = tweak.DescriptionVi,
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 20, 0)
            };
            infoStack.Children.Add(descViText);

            // Admin warning / Cảnh báo admin
            if (tweak.RequiresAdmin)
            {
                var adminPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
                
                var shieldIcon = new TextBlock 
                { 
                    Text = "🛡️", 
                    FontSize = 12, 
                    Margin = new Thickness(0, 0, 6, 0) 
                };
                
                var adminWarning = new TextBlock
                {
                    Text = "Cần quyền Admin / Administrator",
                    FontSize = 11,
                    Foreground = (Brush)Application.Current.Resources["AccentWarningBrush"]
                };
                
                adminPanel.Children.Add(shieldIcon);
                adminPanel.Children.Add(adminWarning);
                infoStack.Children.Add(adminPanel);
            }

            Grid.SetColumn(infoStack, 0);
            grid.Children.Add(infoStack);

            // Right side - Toggle / Bên phải - Công tắc
            var toggleButton = new System.Windows.Controls.CheckBox
            {
                IsChecked = isEnabled,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Style = (Style)FindResource("ToggleSwitchStyle")
            };

            toggleButton.Click += (sender, e) => OnTweakToggle(tweak, toggleButton);

            Grid.SetColumn(toggleButton, 1);
            grid.Children.Add(toggleButton);

            card.Child = grid;
            return card;
        }

        private void OnTweakToggle(RegistryTweak tweak, System.Windows.Controls.CheckBox checkBox)
        {
            bool enable = checkBox.IsChecked ?? false;
            
            var result = _tweaksService.ApplyTweak(tweak, enable);

            if (result.Success)
            {
                StatusText.Text = result.Message;
                MessageBox.Show(
                    result.Message + "\n\nLưu ý: Một số thay đổi cần khởi động lại / Note: Some changes may require restart",
                    "Thành công / Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                StatusText.Text = result.Message;
                MessageBox.Show(
                    result.Message,
                    "Lỗi / Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                
                // Revert checkbox / Hoàn nguyên checkbox
                checkBox.IsChecked = !enable;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
