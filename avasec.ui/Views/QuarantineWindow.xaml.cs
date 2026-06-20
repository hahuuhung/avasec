using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using AVASec.Antivirus.Services;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.UI.Views
{
    public partial class QuarantineWindow : Window
    {
        private readonly QuarantineService _quarantineService;
        private readonly IReportingService _reportingService;
        private List<QuarantinedFile> _files = new();

        public QuarantineWindow()
        {
            InitializeComponent();
            
            // Resolve service from App.ServiceProvider
            _quarantineService = App.ServiceProvider.GetRequiredService<QuarantineService>();
            _reportingService = App.ServiceProvider.GetRequiredService<IReportingService>();
            
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                _files = await _quarantineService.GetQuarantinedFilesAsync();
                QuarantineGrid.ItemsSource = _files;

                if (_files.Count == 0)
                {
                    EmptyText.Visibility = Visibility.Visible;
                    QuarantineGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyText.Visibility = Visibility.Collapsed;
                    QuarantineGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data / Lỗi tải dữ liệu: {ex.Message}", "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = QuarantineGrid.SelectedItem as QuarantinedFile;
            if (selectedFile == null)
            {
                MessageBox.Show("Please select a file to restore / Vui lòng chọn file để khôi phục", "Info / Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to restore:\n{selectedFile.FilePath}?\n\nBạn có chắc muốn khôi phục không?", 
                "Confirm Restore / Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var result = await _quarantineService.RestoreFileAsync(selectedFile.FileId);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = QuarantineGrid.SelectedItem as QuarantinedFile;
            if (selectedFile == null)
            {
                MessageBox.Show("Please select a file to delete / Vui lòng chọn file để xóa", "Info / Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY delete:\n{selectedFile.ThreatName}?\nThis action cannot be undone.\n\nBạn có chắc muốn xóa VĨNH VIỄN không?", 
                "Confirm Delete / Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var result = await _quarantineService.DeleteFileAsync(selectedFile.FileId);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Count == 0) return;

            var confirm = MessageBox.Show("Are you sure you want to delete ALL quarantined files? This action cannot be undone.\n\nBạn có chắc muốn xóa TẤT CẢ file cách ly không?", 
                "Confirm Clear All / Xác nhận xóa tất cả", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var result = await _quarantineService.ClearAllAsync();
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error / Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"AVA Security_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Get data / Lấy dữ liệu
                    var history = await _quarantineService.GetRecentScanHistoryAsync();
                    var quarantine = await _quarantineService.GetQuarantinedFilesAsync();

                    // Generate / Tạo báo cáo
                    _reportingService.GenerateSystemReport(dialog.FileName, history, quarantine);

                    MessageBox.Show("Report exported successfully! / Xuất báo cáo thành công!", "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Open file / Mở file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"Error exporting report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
