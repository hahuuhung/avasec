using AVASec.Optimization.Services;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System;

namespace AVASec.UI.Views
{
    /// <summary>
    /// Process Manager Window / Cửa sổ quản lý tiến trình
    /// </summary>
    public partial class ProcessManagerWindow : Window
    {
        private readonly ProcessService _processService;
        private readonly DispatcherTimer _refreshTimer;
        private ObservableCollection<ProcessInfo> _processes;

        public ProcessManagerWindow()
        {
            InitializeComponent();
            _processService = new ProcessService();
            _processes = new ObservableCollection<ProcessInfo>();
            ProcessListView.ItemsSource = _processes;

            // Auto refresh every 3 seconds / Tự động làm mới mỗi 3 giây
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += (s, e) => RefreshProcessList();

            Loaded += (s, e) => {
                RefreshProcessList();
                _refreshTimer.Start();
            };

            Closed += (s, e) => _refreshTimer.Stop();
        }

        /// <summary>
        /// Refresh process list / Làm mới danh sách tiến trình
        /// </summary>
        private void RefreshProcessList()
        {
            try
            {
                // Save current selection ID / Lưu ID tiến trình đang được chọn
                int selectedId = -1;
                if (ProcessListView.SelectedItem is ProcessInfo current)
                {
                    selectedId = current.Id;
                }

                var processes = _processService.GetRunningProcesses();
                _processes.Clear();
                
                ProcessInfo? toSelect = null;
                foreach (var p in processes)
                {
                    _processes.Add(p);
                    if (p.Id == selectedId)
                    {
                        toSelect = p;
                    }
                }

                // Restore selection / Khôi phục lựa chọn
                if (toSelect != null)
                {
                    ProcessListView.SelectedItem = toSelect;
                }

                StatusText.Text = $"Tổng: {_processes.Count} tiến trình / Total: {_processes.Count} processes";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Lỗi / Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Refresh button click / Nhấn nút làm mới
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        /// <summary>
        /// End task button click / Nhấn nút kết thúc tiến trình
        /// </summary>
        private void EndTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListView.SelectedItem is ProcessInfo selectedProcess)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn kết thúc tiến trình '{selectedProcess.Name}' (ID: {selectedProcess.Id})?\n" +
                    $"Are you sure you want to end process '{selectedProcess.Name}' (ID: {selectedProcess.Id})?",
                    "Xác nhận / Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _processService.KillProcess(selectedProcess.Id);
                    if (success)
                    {
                        MessageBox.Show(
                            $"Đã kết thúc tiến trình '{selectedProcess.Name}'.\nProcess '{selectedProcess.Name}' has been terminated.",
                            "Thành công / Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        RefreshProcessList();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Không thể kết thúc tiến trình '{selectedProcess.Name}'. Có thể cần quyền Admin.\n" +
                            $"Could not terminate process '{selectedProcess.Name}'. Admin rights may be required.",
                            "Lỗi / Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    "Vui lòng chọn một tiến trình để kết thúc.\nPlease select a process to end.",
                    "Thông báo / Notice",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
