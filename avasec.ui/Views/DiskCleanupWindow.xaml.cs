using AVASec.Optimization.Services;
using System;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AVASec.UI.Views
{
    public partial class DiskCleanupWindow : Window
    {
        private readonly DiskCleanerService _cleanerService;
        private ObservableCollection<CategoryCardViewModel> _categories = new ObservableCollection<CategoryCardViewModel>();

        public DiskCleanupWindow()
        {
            InitializeComponent();
            _cleanerService = new DiskCleanerService();
            CategoriesPanel.ItemsSource = _categories;
            Loaded += DiskCleanupWindow_Loaded;
        }

        private async void DiskCleanupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ScanLocations();
        }

        private async Task ScanLocations()
        {
            CleanButton.IsEnabled = false;
            ScanButton.IsEnabled = false;
            StatusText.Text = FindResource("Lang.DiskCleanup.Scanning") as string;
            TotalSizeText.Text = "0.00 MB";
            
            var locations = await Task.Run(() => _cleanerService.GetCleanableLocations());
            
            // Group into categories with enhanced metadata
            _categories.Clear();
            var groups = locations
                .GroupBy(l => l.Category)
                .Select(g => new CategoryCardViewModel
                {
                    Name = g.Key,
                    Description = GetCategoryDescription(g.Key),
                    Locations = g.ToList(),
                    IsChecked = true,
                    EstimatedSize = g.Sum(l => l.EstimatedSize)
                })
                .ToList();

            foreach (var cat in groups)
            {
                _categories.Add(cat);
            }
            
            UpdateTotalStats();
            StatusText.Text = FindResource("Lang.DiskCleanup.Ready") as string;
            CleanButton.IsEnabled = true;
            ScanButton.IsEnabled = true;
        }

        private string GetCategoryDescription(string category)
        {
            return category switch
            {
                "Temp" => "Temporary files and cache / Tệp tạm và bộ nhớ đệm",
                "Downloads" => "Downloaded files / Tệp tải xuống",
                "Recycle" => "Recycle Bin / Thùng rác",
                "Logs" => "System logs / Nhật ký hệ thống",
                "Browsers" => "Browser history and cache / Lịch sử và bộ nhớ đệm trình duyệt",
                "Applications" => "App specific temporary files / Tệp tạm của ứng dụng",
                "Large Files" => "Files over 100MB / Các tệp lớn trên 100MB",
                "Documents" => "Document history and unsaved files / Lịch sử tài liệu và tệp chưa lưu",
                _ => "System cleanup / Dọn dẹp hệ thống"
            };
        }

        private void UpdateTotalStats()
        {
            long totalBytes = _categories.Where(c => c.IsChecked).Sum(c => c.EstimatedSize);
            int categoryCount = _categories.Count(c => c.IsChecked);
            
            TotalSizeText.Text = FormatSize(totalBytes);
            FileCountText.Text = string.Format(FindResource("Lang.DiskCleanup.FilesSelected") as string ?? "{0} files", _categories.Sum(c => c.Locations.Count));
            CategoryCountText.Text = string.Format(FindResource("Lang.DiskCleanup.CategoryCount") as string ?? "{0} categories", categoryCount);

            // Sync Quick Toggle (without triggering its event)
            var recycleCat = _categories.FirstOrDefault(c => c.Name == "Recycle");
            if (recycleCat != null && RecycleQuickToggle != null)
            {
                RecycleQuickToggle.Checked -= RecycleQuickToggle_Changed;
                RecycleQuickToggle.Unchecked -= RecycleQuickToggle_Changed;
                RecycleQuickToggle.IsChecked = recycleCat.IsChecked;
                RecycleQuickToggle.Checked += RecycleQuickToggle_Changed;
                RecycleQuickToggle.Unchecked += RecycleQuickToggle_Changed;
            }
        }

        private void RecycleQuickToggle_Changed(object sender, RoutedEventArgs e)
        {
            var recycleCat = _categories.FirstOrDefault(c => c.Name == "Recycle");
            if (recycleCat != null)
            {
                recycleCat.IsChecked = RecycleQuickToggle.IsChecked == true;
                UpdateTotalStats();
            }
        }

        private void CleanNow_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = _categories
                .Where(c => c.IsChecked)
                .SelectMany(c => c.Locations)
                .Select(l => l.Id)
                .ToList();
            
            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Please select categories to clean. / Vui lòng chọn danh mục để dọn dẹp.", 
                    "Disk Cleanup", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            long totalBytes = _categories.Where(c => c.IsChecked).Sum(c => c.EstimatedSize);
            ConfirmMessageText.Text = FindResource("Lang.DiskCleanup.ConfirmMsg") as string;
            ConfirmationOverlay.Visibility = Visibility.Visible;
        }

        private void CancelCleanup_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ConfirmCleanNow_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationOverlay.Visibility = Visibility.Collapsed;
            
            var selectedIds = _categories
                .Where(c => c.IsChecked)
                .SelectMany(c => c.Locations)
                .Select(l => l.Id)
                .ToList();

            CleanButton.IsEnabled = false;
            StatusText.Text = FindResource("Lang.DiskCleanup.Cleaning") as string;

            var result = await _cleanerService.CleanSelectedLocationsAsync(selectedIds);

            StatusText.Text = FindResource("Lang.DiskCleanup.Complete") as string;
            MessageBox.Show(string.Format(FindResource("Lang.DiskCleanup.Complete") as string + ": {0}", FormatSize(result.BytesCleaned)), 
                FindResource("Lang.DiskCleanup.Complete") as string, MessageBoxButton.OK, MessageBoxImage.Information);
            
            await ScanLocations();
        }

        private async void Rescan_Click(object sender, RoutedEventArgs e)
        {
            await ScanLocations();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
                // Simple Login Window launch if not using Overlay
                // Or inform user to use Dashboard
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
                 var settingsWin = new SettingsView(); // Assuming parameterless constructor or handled by DI if possible
                 // If SettingsView needs DI, we should use:
                 // var settingsWin = App.ServiceProvider.GetRequiredService<SettingsView>();
                 // But DiskCleanup doesn't have easy DI access unless we use App.xaml.cs static property (which we do: App.ServiceProvider)
                 
                 // Using simple instantiation for now or DI if confirmed
                 // Dashboard uses: App.ServiceProvider.GetRequiredService<SettingsView>();
                 // Let's try that to be consistent
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

        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTotalStats();
        }

        private void Category_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateTotalStats();
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cat in _categories) cat.IsChecked = true;
            UpdateTotalStats();
        }

        private void CheckNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cat in _categories) cat.IsChecked = false;
            UpdateTotalStats();
        }

        private void CheckDefault_Click(object sender, RoutedEventArgs e)
        {
            // Set safe defaults (Temp, Recycle)
            foreach (var cat in _categories)
            {
                cat.IsChecked = cat.Name == "Temp" || cat.Name == "Recycle";
            }
            UpdateTotalStats();
        }

        private string FormatSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            double dblBytes = bytes;
            int i = 0;
            while (dblBytes >= 1024 && i < suffix.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }
            return $"{dblBytes:0.##} {suffix[i]}";
        }

        private void CategoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CategoryCardViewModel selectedCat)
            {
                // Manage selection visual
                foreach (var cat in _categories)
                {
                    cat.IsSelected = (cat == selectedCat);
                }

                LoadCategoryFiles(selectedCat);
            }
        }

        private void LoadCategoryFiles(CategoryCardViewModel category)
        {
            DetailsTitle.Text = $"{category.Name} - {FindResource("Lang.DiskCleanup.Details")}";
            
            var allItems = new List<CleanupItem>();
            foreach (var loc in category.Locations)
            {
                allItems.AddRange(_cleanerService.GetCleanupItems(loc.Id));
            }

            FilesListView.ItemsSource = allItems.OrderByDescending(f => f.Size).ToList();
            DetailsCount.Text = string.Format(FindResource("Lang.DiskCleanup.Items") as string ?? "{0} items", allItems.Count);
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesListView.SelectedItem is CleanupItem selectedFile)
            {
                UpdatePreview(selectedFile);
            }
        }

        private void UpdatePreview(CleanupItem file)
        {
            try
            {
                PreviewFileName.Text = file.FileName;
                PreviewFileSize.Text = file.SizeStr;
                PreviewFileType.Text = Path.GetExtension(file.FilePath).ToUpper();

                string ext = Path.GetExtension(file.FilePath).ToLower();
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico" };

                if (imageExtensions.Contains(ext) && File.Exists(file.FilePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file.FilePath);
                    bitmap.DecodePixelWidth = 240; // Optimization
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    PreviewImage.Source = bitmap;
                    PreviewImage.Visibility = Visibility.Visible;
                    NoPreviewContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowNoPreview();
                }
            }
            catch
            {
                ShowNoPreview();
            }
        }

        private void ShowNoPreview()
        {
            PreviewImage.Visibility = Visibility.Collapsed;
            NoPreviewContainer.Visibility = Visibility.Visible;
        }
    }

    public class CategoryCardViewModel : INotifyPropertyChanged
    {
        private bool _isChecked;
        private bool _isSelected;
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CleanupLocation> Locations { get; set; } = new List<CleanupLocation>();
        public long EstimatedSize { get; set; }
        
        public string EstimatedSizeStr => FormatSize(EstimatedSize);
        public double SpacePercentage => Math.Min((EstimatedSize / (1024.0 * 1024.0 * 100)) * 100, 100); // Relative to 100MB cap

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string FormatSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            double dblBytes = bytes;
            int i = 0;
            while (dblBytes >= 1024 && i < suffix.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }
            return $"{dblBytes:0.##} {suffix[i]}";
        }
    }
}
