using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AVASec.Core.Services;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Interaction logic for PrivacyGuardianView.xaml
    /// </summary>
    public partial class PrivacyGuardianView : UserControl
    {
        private readonly PrivacyGuardianService _privacyService;
        private PrivacyGuardianService.PrivacyReport? _lastReport;

        public PrivacyGuardianView()
        {
            InitializeComponent();
            _privacyService = new PrivacyGuardianService();
        }

        private async void ScanPrivacy_Click(object sender, RoutedEventArgs e)
        {
            ScanBtn.IsEnabled = false;
            ScanBtn.Content = "🔄 Scanning...";
            StatusText.Text = "Scanning privacy settings...";

            try
            {
                var status = new Progress<string>(s => StatusText.Text = s);
                _lastReport = await _privacyService.RunPrivacyScanAsync(status);

                UpdateUI(_lastReport);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Privacy scan failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ScanBtn.IsEnabled = true;
                ScanBtn.Content = "🔍 Scan Privacy";
            }
        }

        private void UpdateUI(PrivacyGuardianService.PrivacyReport report)
        {
            // Update score
            ScoreText.Text = report.PrivacyScore.ToString();
            GradeText.Text = report.Grade;
            
            // Update stats
            BlockedCountText.Text = report.TrackersBlocked.ToString();
            IssuesCountText.Text = report.BrowserIssues.Count.ToString();

            // Update progress ring
            double normalizedScore = report.PrivacyScore / 100.0;
            double circumference = Math.PI * 100;
            double dashLength = normalizedScore * circumference;
            ScoreRing.StrokeDashArray = new System.Windows.Media.DoubleCollection { dashLength / 10, 100 };

            // Update tracker list
            TrackerList.ItemsSource = report.BlockedDomains.Take(20).ToList();
            if (report.BlockedDomains.Count == 0)
            {
                TrackerList.ItemsSource = new List<string> { "No trackers blocked yet. Click 'Enable Tracking Protection' to start." };
            }

            // Update browser issues
            if (report.BrowserIssues.Any())
            {
                BrowserIssuesList.ItemsSource = report.BrowserIssues;
                BrowserIssuesList.Visibility = Visibility.Visible;
                NoBrowserIssuesText.Visibility = Visibility.Collapsed;
            }
            else
            {
                BrowserIssuesList.Visibility = Visibility.Collapsed;
                NoBrowserIssuesText.Visibility = Visibility.Visible;
            }

            // Update telemetry list
            TelemetryList.ItemsSource = report.TelemetryStatus;

            StatusText.Text = $"Scan complete. Privacy Score: {report.PrivacyScore}%";
        }

        private async void EnableBlocking_Click(object sender, RoutedEventArgs e)
        {
            EnableBlockingBtn.IsEnabled = false;
            EnableBlockingBtn.Content = "🔄 Enabling...";

            try
            {
                var status = new Progress<string>(s => StatusText.Text = s);
                int count = await _privacyService.EnableTrackingProtectionAsync(status);

                MessageBox.Show(
                    $"Tracking protection enabled!\n\n" +
                    $"Blocked {count} tracking domains.\n\n" +
                    $"You may need to restart your browser for changes to take effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh scan
                ScanPrivacy_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to enable tracking protection:\n\n{ex.Message}\n\n" +
                    "Try running the application as Administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                EnableBlockingBtn.IsEnabled = true;
                EnableBlockingBtn.Content = "🛡️ Enable Tracking Protection";
            }
        }

        private async void DisableBlocking_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to disable tracking protection?\n\n" +
                "This will remove all AVA Security blocking rules from your hosts file.",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            DisableBlockingBtn.IsEnabled = false;

            try
            {
                int count = await _privacyService.DisableTrackingProtectionAsync();

                MessageBox.Show(
                    $"Tracking protection disabled.\n\n" +
                    $"Removed {count} blocking rules.",
                    "Done",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh scan
                ScanPrivacy_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DisableBlockingBtn.IsEnabled = true;
            }
        }

        private async void DisableTelemetry_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will disable Windows telemetry settings.\n\n" +
                "⚠️ Requires Administrator privileges.\n" +
                "⚠️ Some Windows features may be affected.\n\n" +
                "Do you want to continue?",
                "Disable Telemetry",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            DisableTelemetryBtn.IsEnabled = false;

            try
            {
                var status = new Progress<string>(s => StatusText.Text = s);
                int count = await _privacyService.DisableTelemetryAsync(status);

                MessageBox.Show(
                    $"Telemetry settings updated!\n\n" +
                    $"Modified {count} settings.\n\n" +
                    "A restart may be required for changes to take full effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh scan
                ScanPrivacy_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to disable telemetry:\n\n{ex.Message}\n\n" +
                    "Please run the application as Administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                DisableTelemetryBtn.IsEnabled = true;
            }
        }
    }
}
