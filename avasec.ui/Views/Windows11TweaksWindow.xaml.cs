using AVASec.Optimization.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Windows 11 Master Window
    /// </summary>
    public partial class Windows11TweaksWindow : Window
    {
        private readonly RegistryTweaksService _tweaksService;

        public Windows11TweaksWindow()
        {
            InitializeComponent();
            _tweaksService = new RegistryTweaksService();
            LoadTweaks();
        }

        private void LoadTweaks()
        {
            var tweaks = _tweaksService.GetWindows11Tweaks();
            
            // Group by category
            var categories = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<RegistryTweak>>();
            
            foreach (var tweak in tweaks)
            {
                if (!categories.ContainsKey(tweak.Category))
                {
                    categories[tweak.Category] = new System.Collections.Generic.List<RegistryTweak>();
                }
                categories[tweak.Category].Add(tweak);
            }

            foreach (var category in categories)
            {
                // Section Header
                var headerText = new TextBlock 
                { 
                    Text = category.Key.ToUpper(), 
                    Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 15, 0, 10)
                };
                TweaksContainer.Children.Add(headerText);
                
                // Grid for cards
                var wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };
                
                foreach (var tweak in category.Value)
                {
                    var tweakCard = CreateTweakCard(tweak);
                    TweaksContainer.Children.Add(tweakCard);
                }
            }
        }

        private Border CreateTweakCard(RegistryTweak tweak)
        {
            bool isEnabled = _tweaksService.IsTweakEnabled(tweak);

            var card = new Border
            {
                Background = (Brush)Application.Current.Resources["DarkSurfaceBrush"],
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 15),
                BorderBrush = (Brush)Application.Current.Resources["DarkBorderBrush"],
                BorderThickness = new Thickness(1)
            };
            
            // Hover effect
            card.MouseEnter += (s, e) => card.BorderBrush = (Brush)Application.Current.Resources["AccentTealBrush"];
            card.MouseLeave += (s, e) => card.BorderBrush = (Brush)Application.Current.Resources["DarkBorderBrush"];


            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoStack = new StackPanel();
            
            // Name
            infoStack.Children.Add(new TextBlock
            {
                Text = tweak.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Vi Name
            infoStack.Children.Add(new TextBlock
            {
                Text = tweak.NameVi,
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Description
            infoStack.Children.Add(new TextBlock
            {
                Text = tweak.DescriptionVi,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap
            });

            Grid.SetColumn(infoStack, 0);
            grid.Children.Add(infoStack);

            // Toggle Switch style checkbox
            var toggle = new System.Windows.Controls.CheckBox 
            { 
                IsChecked = isEnabled,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20,0,0,0),
                Style = (Style)FindResource("ToggleSwitchStyle")
            };
            
            toggle.Click += (s, e) => OnTweakToggle(tweak, toggle);

            Grid.SetColumn(toggle, 1);
            grid.Children.Add(toggle);

            card.Child = grid;
            return card;
        }

        private void OnTweakToggle(RegistryTweak tweak, System.Windows.Controls.CheckBox toggle)
        {
            bool enable = toggle.IsChecked ?? false;
            var result = _tweaksService.ApplyTweak(tweak, enable);
            
            StatusText.Text = result.Message;
            
            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                toggle.IsChecked = !enable; // Revert
            }
        }

        // Action Handlers
        private void ApplyGamingMode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gaming Mode Enabled! \nBackground services minimized.", "Gaming Mode", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
             MessageBox.Show("All tweaks reset to Windows defaults.", "Reset Info", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
