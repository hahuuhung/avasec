using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using AVASec.Core.Services;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Interaction logic for GameModeView.xaml
    /// </summary>
    public partial class GameModeView : UserControl
    {
        private readonly GameModeUltraService _gameModeService;
        private bool _isEnabled = false;

        public GameModeView()
        {
            InitializeComponent();
            _gameModeService = new GameModeUltraService();
            
            // Initial game detection
            RefreshGamesAsync();
        }

        private async void PowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isEnabled)
            {
                // Disable Game Mode
                await DisableGameModeAsync();
            }
            else
            {
                // Enable Game Mode
                await EnableGameModeAsync();
            }
        }

        private async System.Threading.Tasks.Task EnableGameModeAsync()
        {
            PowerButtonLabel.Text = "ACTIVATING...";
            
            // Get selected profile
            string profile = "Performance";
            if (ProfileBalanced.IsChecked == true) profile = "Balanced";
            else if (ProfilePerformance.IsChecked == true) profile = "Performance";
            else if (ProfileUltra.IsChecked == true) profile = "Ultra";
            else if (ProfileStream.IsChecked == true) profile = "Stream";

            var status = new Progress<string>(s => StatusText.Text = s);

            try
            {
                var result = await _gameModeService.EnableGameModeAsync(profile, status);
                
                if (result.IsEnabled)
                {
                    _isEnabled = true;
                    UpdateUIForEnabled(result);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to enable Game Mode: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task DisableGameModeAsync()
        {
            PowerButtonLabel.Text = "DISABLING...";
            
            var status = new Progress<string>(s => StatusText.Text = s);

            try
            {
                var result = await _gameModeService.DisableGameModeAsync(status);
                
                _isEnabled = false;
                UpdateUIForDisabled();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateUIForEnabled(GameModeUltraService.GameModeStatus result)
        {
            // Update badge
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
            StatusBadgeText.Text = "ON";

            // Update power button visual
            PowerButtonLabel.Text = "CLICK TO DISABLE";

            // Update power button glow to green
            var glowEffect = new DropShadowEffect
            {
                BlurRadius = 40,
                ShadowDepth = 0,
                Opacity = 0.8,
                Color = Color.FromRgb(16, 185, 129)
            };
            PowerButton.Effect = glowEffect;

            // Update text
            ActiveProfileText.Text = $"Profile: {result.ActiveProfile}";
            StatusText.Text = $"Game Mode Ultra active since {result.EnabledAt:HH:mm}";

            // Update stats
            GamesDetectedText.Text = result.DetectedGames.Count.ToString();
            OptimizedText.Text = result.OptimizedProcesses.ToString();
            MemoryFreedText.Text = $"{result.MemoryFreedMB:F0} MB";
            PowerPlanText.Text = result.PowerPlan;

            // Update games list
            if (result.DetectedGames.Any())
            {
                GamesList.ItemsSource = result.DetectedGames;
                GamesList.Visibility = Visibility.Visible;
                NoGamesText.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateUIForDisabled()
        {
            // Update badge
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            StatusBadgeText.Text = "OFF";

            // Update power button visual
            PowerButtonLabel.Text = "CLICK TO ENABLE";
            PowerButton.Effect = new DropShadowEffect
            {
                BlurRadius = 30,
                ShadowDepth = 0,
                Opacity = 0.5,
                Color = Color.FromRgb(239, 68, 68)
            };

            // Update text
            ActiveProfileText.Text = "Profile: Not Active";
            StatusText.Text = "Game Mode is currently disabled";

            // Reset stats
            GamesDetectedText.Text = "0";
            OptimizedText.Text = "0";
            MemoryFreedText.Text = "0 MB";
            PowerPlanText.Text = "Balanced";
        }

        private async void RefreshGames_Click(object sender, RoutedEventArgs e)
        {
            await RefreshGamesAsync();
        }

        private async System.Threading.Tasks.Task RefreshGamesAsync()
        {
            RefreshBtn.IsEnabled = false;
            RefreshBtn.Content = "🔄 Scanning...";

            try
            {
                var games = await _gameModeService.DetectGamesAsync();
                
                if (games.Any())
                {
                    GamesList.ItemsSource = games;
                    GamesList.Visibility = Visibility.Visible;
                    NoGamesText.Visibility = Visibility.Collapsed;
                    GamesDetectedText.Text = games.Count.ToString();
                }
                else
                {
                    GamesList.Visibility = Visibility.Collapsed;
                    NoGamesText.Visibility = Visibility.Visible;
                    GamesDetectedText.Text = "0";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Scan error: {ex.Message}";
            }
            finally
            {
                RefreshBtn.IsEnabled = true;
                RefreshBtn.Content = "🔄 Refresh Games";
            }
        }
    }
}
