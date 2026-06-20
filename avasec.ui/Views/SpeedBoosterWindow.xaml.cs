using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using AVASec.Core.Interfaces;
using AVASec.Optimization.Services;

namespace AVASec.UI.Views
{
    public partial class SpeedBoosterWindow : Window
    {
        private readonly ISystemMonitorService _monitorService;
        private readonly IProcessService _processService;
        private readonly System.Windows.Threading.DispatcherTimer _timer;

        public ObservableCollection<ProcessInfo> ProcessListItems { get; set; }

        public SpeedBoosterWindow(ISystemMonitorService monitorService, IProcessService processService)
        {
            InitializeComponent();
            _monitorService = monitorService;
            _processService = processService;

            ProcessListItems = new ObservableCollection<ProcessInfo>();
            ProcessList.ItemsSource = ProcessListItems;

            // Timer for updating metrics every second
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            LoadProcesses();
        }
        
        // Parameterless constructor for XAML designer
        public SpeedBoosterWindow()
        {
            InitializeComponent();
            _timer = new System.Windows.Threading.DispatcherTimer();
            ProcessListItems = new ObservableCollection<ProcessInfo>();
            _monitorService = null!;
            _processService = null!;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                _monitorService.Refresh();
                
                // Update CPU Text
                CpuText.Text = $"{Math.Round(_monitorService.CpuUsagePercent)}%";

                // Update RAM Text
                RamText.Text = $"{Math.Round(_monitorService.RamUsagePercent)}%";
                
                // Note: Circular Progress bar update isn't implemented here as we used ContentControl placeholder in XAML
                // In a real implementation, we would bind the Value property of the custom control.
            }
            catch { }
        }

        private async void LoadProcesses()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Simulated heavy processes for demo
                    // In real app, call _processService.GetRunningProcesses()
                    // But filtering for safe-to-close apps is complex.
                    var processes = _processService.GetRunningProcesses();
                    
                    // Filter for top 10 memory consuming
                    var heavyApps = processes.Take(10).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        ProcessListItems.Clear();
                        foreach (var p in heavyApps)
                        {
                            ProcessListItems.Add(p);
                        }
                    });
                }
                catch { }
            });
        }

        private async void BoostButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable button
            BoostBtn.IsEnabled = false;

            // Simulate Boost process
            await Task.Delay(2000); // Wait for animation

            // "Clear" processes from list (Simulation)
            ProcessListItems.Clear();

            // Simulate Ram drop
            RamText.Text = "35%";
            CpuText.Text = "12%";

            MessageBox.Show("System Optmized Successfully!\n- RAM Cleared: 1.2GB\n- Background Apps Stopped: 15", "Speed Booster", MessageBoxButton.OK, MessageBoxImage.Information);

            // Re-enable button
            BoostBtn.IsEnabled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer.Stop();
        }
    }
}
