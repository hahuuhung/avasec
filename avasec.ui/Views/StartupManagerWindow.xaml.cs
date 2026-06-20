using AVASec.Optimization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Startup Manager Window (AVG TuneUp style) / Cửa sổ Quản lý Khởi động
    /// </summary>
    public partial class StartupManagerWindow : Window
    {
        private readonly StartupManagerService _startupService;
        private List<StartupItem> _items = new List<StartupItem>();
        private HashSet<string> _ignoredItems = new HashSet<string>();

        // Icon colors based on app type / Màu icon theo loại app
        private static readonly System.Windows.Media.Color[] IconColors = new[]
        {
            Color.FromRgb(255, 107, 107), // Red
            Color.FromRgb(78, 205, 196),  // Teal
            Color.FromRgb(74, 144, 217),  // Blue
            Color.FromRgb(255, 179, 71),  // Orange
            Color.FromRgb(170, 150, 218), // Purple
            Color.FromRgb(0, 212, 170),   // Green
            Color.FromRgb(255, 193, 7),   // Yellow
            Color.FromRgb(156, 39, 176),  // Deep Purple
        };

        public StartupManagerWindow()
        {
            InitializeComponent();
            _startupService = new StartupManagerService();
            LoadStartupItems();
        }

        private async void LoadStartupItems()
        {
            // Show loading state
            ProgramCountText.Text = "Đang quét... / Scanning...";
            ItemsPanel.Children.Clear();
            
            // Add loading indicator
            var loadingText = new TextBlock
            {
                Text = "⏳ Đang tải danh sách chương trình... / Loading programs...",
                Foreground = new SolidColorBrush(Color.FromRgb(142, 142, 158)),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            ItemsPanel.Children.Add(loadingText);

            await Task.Run(() =>
            {
                _items = _startupService.GetStartupPrograms();
                
                // Calculate impact for each item
                foreach (var item in _items)
                {
                    item.Impact = CalculateImpact(item);
                }
            });

            Dispatcher.Invoke(() =>
            {
                var activeItems = _items.Where(i => !_ignoredItems.Contains(i.Name)).ToList();
                ProgramCountText.Text = $"{activeItems.Count} chương trình đang làm chậm PC của bạn / {activeItems.Count} programs are slowing down your PC";
                PopulateItems();
            });
        }

        private string CalculateImpact(StartupItem item)
        {
            // Calculate impact based on location and type
            if (item.Command.Contains("System32") || item.Command.Contains("Windows"))
                return "High";
            else if (item.Command.Contains("Program Files") || item.Command.Contains("Microsoft"))
                return "Medium";
            else
                return "Low";
        }

        private void PopulateItems()
        {
            ItemsPanel.Children.Clear();

            var visibleItems = _items.Where(i => !_ignoredItems.Contains(i.Name)).ToList();

            // Sort by impact (High first)
            visibleItems = visibleItems
                .OrderByDescending(i => i.Impact == "High")
                .ThenByDescending(i => i.Impact == "Medium")
                .ThenBy(i => i.Name)
                .ToList();

            int index = 0;
            foreach (var item in visibleItems)
            {
                var itemControl = CreateItemControl(item, index);
                ItemsPanel.Children.Add(itemControl);
                index++;
            }

            // Add empty state if no items
            if (!visibleItems.Any())
            {
                var emptyText = new TextBlock
                {
                    Text = "✅ Không có chương trình nào làm chậm PC!\nNo programs slowing down your PC!",
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 170)),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                ItemsPanel.Children.Add(emptyText);
            }
        }

        private FrameworkElement CreateItemControl(StartupItem item, int index)
        {
            var border = new Border
            {
                BorderBrush = (Brush)Application.Current.Resources["DarkBorderBrush"],
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 16, 15, 16),
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 4, 0, 4)
            };

            // Hover effect
            border.MouseEnter += (s, e) => border.Background = (Brush)Application.Current.Resources["DarkSurfaceBrush"];
            border.MouseLeave += (s, e) => border.Background = Brushes.Transparent;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

            // Column 1: Icon + Name
            var nameStack = new StackPanel { Orientation = Orientation.Horizontal };
            
            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(10),
                Background = GetIconColor(item.Name),
                Margin = new Thickness(0, 0, 15, 0)
            };
            
            // Add subtle shadow to icon
            iconBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect 
            { 
                Color = ((SolidColorBrush)iconBorder.Background).Color, 
                BlurRadius = 10, 
                ShadowDepth = 0, 
                Opacity = 0.3 
            };

            iconBorder.Child = new TextBlock
            {
                Text = item.Name.Length > 0 ? item.Name[0].ToString().ToUpper() : "?",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var namePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            
            var nameText = new TextBlock
            {
                Text = TruncateText(item.Name, 35),
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                ToolTip = item.Name
            };

            var locationText = new TextBlock
            {
                Text = item.Location,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0)
            };

            namePanel.Children.Add(nameText);
            namePanel.Children.Add(locationText);

            nameStack.Children.Add(iconBorder);
            nameStack.Children.Add(namePanel);
            Grid.SetColumn(nameStack, 0);

            // Column 2: Usage Statistics
            var usageStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var usageText = new TextBlock
            {
                Text = $"{item.UsageCount} lần dùng / used",
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            var usageLabel = new TextBlock
            {
                Text = GetUsageIntensity(item.UsageCount),
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                FontSize = 10
            };
            usageStack.Children.Add(usageText);
            usageStack.Children.Add(usageLabel);
            Grid.SetColumn(usageStack, 1);

            // Column 3: Severity Bar with animation
            var severityStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 20, 0)
            };

            double severity = CalculateSeverity(item);
            var severityBar = CreateSeverityBar(severity, index);
            severityStack.Children.Add(severityBar);
            
            // Text label for severity
            var severityLabel = new TextBlock
            {
                Text = GetSeverityLabel(severity),
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                FontSize = 11,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            severityStack.Children.Add(severityLabel);
            
            Grid.SetColumn(severityStack, 2);

            // Column 3: Action Buttons
            var actionStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            var ignoreBtn = new Button
            {
                Content = "Ignore",
                Style = (Style)FindResource("IgnoreLink"),
                Margin = new Thickness(0, 0, 15, 0),
                Tag = item,
                VerticalAlignment = VerticalAlignment.Center
            };
            ignoreBtn.Click += Ignore_Click;

            var sleepBtn = new Button
            {
                Content = item.IsEnabled ? "🌙 SLEEP" : "☀ WAKE",
                Style = (Style)FindResource("SleepButton"),
                Tag = item,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 80
            };
            
            if (!item.IsEnabled)
            {
                // Dimmed style for disabled items
                sleepBtn.Background = (Brush)Application.Current.Resources["SurfaceBrush"];
                sleepBtn.Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"];
            }
            
            sleepBtn.Click += Sleep_Click;

            actionStack.Children.Add(ignoreBtn);
            actionStack.Children.Add(sleepBtn);
            Grid.SetColumn(actionStack, 3);

            grid.Children.Add(nameStack);
            grid.Children.Add(usageStack);
            grid.Children.Add(severityStack);
            grid.Children.Add(actionStack);

            border.Child = grid;

            // Fade in animation
            border.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200 + index * 50));
            border.BeginAnimation(OpacityProperty, fadeIn);

            return border;
        }

        private string GetSeverityLabel(double severity)
        {
            if (severity > 0.7) return "High Impact";
            if (severity > 0.4) return "Medium";
            return "Low Impact";
        }

        private FrameworkElement CreateSeverityBar(double severity, int index)
        {
            var container = new Grid { Width = 200, Height = 6 };

            // Background
            var bgRect = new Rectangle
            {
                Fill = (Brush)Application.Current.Resources["DarkSurfaceBrush"],
                RadiusX = 3,
                RadiusY = 3
            };

            // Severity fill with gradient
            var fillRect = new Rectangle
            {
                Width = 0, // Animate from 0
                HorizontalAlignment = HorizontalAlignment.Left,
                RadiusX = 3,
                RadiusY = 3
            };

            // Color based on severity
            Brush fillBrush;
            if (severity > 0.7)
                fillBrush = (Brush)Application.Current.Resources["ErrorBrush"]; // Red
            else if (severity > 0.4)
                fillBrush = (Brush)Application.Current.Resources["AccentWarningBrush"]; // Orange
            else
                fillBrush = (Brush)Application.Current.Resources["AccentTealBrush"]; // Teal

            fillRect.Fill = fillBrush;

            container.Children.Add(bgRect);
            container.Children.Add(fillRect);

            // Animate width
            var targetWidth = 200 * severity;
            var animation = new DoubleAnimation(0, targetWidth, TimeSpan.FromMilliseconds(400 + index * 30));
            animation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            fillRect.BeginAnimation(WidthProperty, animation);

            return container;
        }

        private double CalculateSeverity(StartupItem item)
        {
            switch (item.Impact)
            {
                case "High": return 0.85 + new Random(item.Name.GetHashCode()).NextDouble() * 0.15;
                case "Medium": return 0.45 + new Random(item.Name.GetHashCode()).NextDouble() * 0.25;
                case "Low": return 0.15 + new Random(item.Name.GetHashCode()).NextDouble() * 0.2;
                default: return new Random(item.Name.GetHashCode()).NextDouble() * 0.7 + 0.2;
            }
        }

        private System.Windows.Media.Brush GetIconColor(string name)
        {
            // Consistent color based on name hash
            int colorIndex = Math.Abs(name.GetHashCode()) % IconColors.Length;
            return new SolidColorBrush(IconColors[colorIndex]);
        }

        private string GetUsageIntensity(int count)
        {
            if (count > 50) return "🔥 Rất thường xuyên / Very High";
            if (count > 10) return "⭐ Thường xuyên / Frequent";
            if (count > 0) return "📉 Ít dùng / Rare";
            return "❔ Chưa rõ / Unknown";
        }

        private string TruncateText(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        // Event Handlers
        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is StartupItem item)
            {
                _ignoredItems.Add(item.Name);
                PopulateItems();
                
                var activeItems = _items.Where(i => !_ignoredItems.Contains(i.Name)).ToList();
                ProgramCountText.Text = $"{activeItems.Count} chương trình đang làm chậm PC của bạn / {activeItems.Count} programs are slowing down your PC";
            }
        }

        private async void Sleep_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is StartupItem item)
            {
                try
                {
                    if (item.IsEnabled)
                    {
                        var res = await _startupService.DisableStartupItemAsync(item);
                        if (res.Success)
                        {
                            LoadStartupItems();
                        }
                    }
                    else
                    {
                        var res = await _startupService.EnableStartupItemAsync(item);
                        if (res.Success)
                        {
                            LoadStartupItems();
                        }
                        else
                        {
                            MessageBox.Show(res.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi: {ex.Message}\nError: {ex.Message}", 
                        "Lỗi / Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PutAllToSleep_Click(object sender, MouseButtonEventArgs e)
        {
            var enabledItems = _items.Where(i => i.IsEnabled && !_ignoredItems.Contains(i.Name)).ToList();
            
            if (!enabledItems.Any())
            {
                MessageBox.Show("Không có chương trình nào để tắt.\nNo programs to disable.",
                    "Thông báo / Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                $"Đưa {enabledItems.Count} chương trình vào trạng thái ngủ?\nPut {enabledItems.Count} programs to sleep?",
                "Xác nhận / Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int success = 0;
                foreach (var item in enabledItems)
                {
                    try
                    {
                        _startupService.DisableStartupProgram(item.Name);
                        success++;
                    }
                    catch { }
                }
                
                MessageBox.Show($"✅ Đã tắt {success}/{enabledItems.Count} chương trình.\nDisabled {success}/{enabledItems.Count} programs.",
                    "Hoàn tất / Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadStartupItems();
            }
        }

        private void Rescan_Click(object sender, RoutedEventArgs e)
        {
            _ignoredItems.Clear();
            LoadStartupItems();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
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
    }
}
