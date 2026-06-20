using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AVASec.Core.Services;

namespace AVASec.UI.Views
{
    public partial class FileRecoveryWindow : Window
    {
        private readonly RecoveryService _recoveryService;
        public ObservableCollection<RecoverableFile> RecoveredFiles { get; set; }
        public ObservableCollection<DriveViewModel> Drives { get; set; }
        private string _selectedDrive;

        public FileRecoveryWindow()
        {
            InitializeComponent();
            _recoveryService = new RecoveryService();
            RecoveredFiles = new ObservableCollection<RecoverableFile>();
            Drives = new ObservableCollection<DriveViewModel>();
            
            FilesListView.ItemsSource = RecoveredFiles;
            DrivesList.ItemsSource = Drives;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDrives();
            TxtOutputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RecoveredFiles");
        }

        private void LoadDrives()
        {
            var drives = _recoveryService.GetDrives();
            foreach (var drive in drives)
            {
                Drives.Add(new DriveViewModel 
                { 
                    Name = drive.Name, 
                    IsReady = false 
                });
            }

            if (Drives.Count > 0)
            {
                Drives[0].IsReady = true; // Select first by default
                _selectedDrive = Drives[0].Name;
            }
        }

        private void Drive_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.Tag != null)
            {
                _selectedDrive = btn.Tag.ToString();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedDrive))
            {
                MessageBox.Show("Please select a drive to scan.", "File Recovery", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnScan.IsEnabled = false;
            RecoveredFiles.Clear();
            ProgressArea.Visibility = Visibility.Visible;
            ScanProgress.IsIndeterminate = true;
            StatusText.Text = (string)TryFindResource("Lang.Recovery.Scanning") ?? "Scanning...";

            try
            {
                var files = await _recoveryService.ScanDriveAsync(_selectedDrive);
                
                foreach (var file in files)
                {
                    RecoveredFiles.Add(file);
                }

                TxtCount.Text = RecoveredFiles.Count.ToString();
                StatusText.Text = $"Scan Complete. Found {RecoveredFiles.Count} files.";
                BtnRecover.IsEnabled = RecoveredFiles.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Scan Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Scan Failed.";
            }
            finally
            {
                BtnScan.IsEnabled = true;
                ScanProgress.IsIndeterminate = false;
                ScanProgress.Value = 100;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            dialog.Title = (string)TryFindResource("Lang.Recovery.Output") ?? "Select Output Folder";
            if (dialog.ShowDialog() == true)
            {
                TxtOutputPath.Text = dialog.FolderName;
            }
        }

        private async void RecoverButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = RecoveredFiles.Where(f => f.IsSelected).ToList();
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Please select files to recover.", "Recovery", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string destPath = TxtOutputPath.Text;
            if (string.IsNullOrWhiteSpace(destPath))
            {
                destPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RecoveredFiles");
            }

            try 
            {
                Directory.CreateDirectory(destPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BtnRecover.IsEnabled = false;
            StatusText.Text = "Recovering files...";
            ScanProgress.Value = 0;
            ScanProgress.IsIndeterminate = false;
            ScanProgress.Visibility = Visibility.Visible;

            try
            {
                await _recoveryService.RecoverFilesAsync(selectedFiles, destPath);
                MessageBox.Show($"Successfully recovered {selectedFiles.Count} files to:\n{destPath}", "Recovery Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Recovery Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnRecover.IsEnabled = true;
                StatusText.Text = "Ready";
                ScanProgress.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class DriveViewModel
    {
        public string Name { get; set; }
        public bool IsReady { get; set; }
    }
}
