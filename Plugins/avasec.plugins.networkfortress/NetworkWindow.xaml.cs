using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Plugins.NetworkFortress
{
    public partial class NetworkWindow : Window
    {
        private readonly IPluginContext? _context;
        private DispatcherTimer _timer;
        private NetworkInterface? _activeInterface;
        private long _bytesSent = 0;
        private long _bytesReceived = 0;
        private double _maxSpeed = 1024 * 1024; // 1MB/s initial scale

        private List<double> _dlHistory = new();
        private List<double> _ulHistory = new();
        private const int MaxPoints = 60;

        private Polyline _dlLine;
        private Polyline _ulLine;

        public NetworkWindow(IPluginContext? context)
        {
            InitializeComponent();
            _context = context;
            
            InitializeGraphObjects(); // Initialize graph lines once / Khởi tạo các đường đồ thị một lần
            InitializeNetwork();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.Closed += (s, e) => _timer.Stop();
        }

        private void InitializeGraphObjects()
        {
            // Initialize Download Line (Green) / Khởi tạo đường Download (Xanh lá)
            _dlLine = new Polyline
            {
                Stroke = Brushes.Lime,
                StrokeThickness = 2,
                Points = new PointCollection()
            };
            GraphCanvas.Children.Add(_dlLine);

            // Initialize Upload Line (Yellow) / Khởi tạo đường Upload (Vàng)
            _ulLine = new Polyline
            {
                Stroke = Brushes.Gold,
                StrokeThickness = 2,
                Points = new PointCollection()
            };
            GraphCanvas.Children.Add(_ulLine);
        }

        private void InitializeNetwork()
        {
            try
            {
                // Find first active interface (Ethernet or WiFi)
                // Tìm giao diện mạng hoạt động đầu tiên (Ethernet hoặc WiFi)
                _activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && 
                                       (n.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                                        n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));

                if (_activeInterface != null)
                {
                    InterfaceText.Text = $"Monitoring: {_activeInterface.Name}";
                    var stats = _activeInterface.GetIPStatistics();
                    _bytesSent = stats.BytesSent;
                    _bytesReceived = stats.BytesReceived;
                }
                else
                {
                    InterfaceText.Text = "No active network interface found.";
                }

                // Init history with zeros / Khởi tạo lịch sử bằng 0
                for(int i=0; i<MaxPoints; i++) { _dlHistory.Add(0); _ulHistory.Add(0); }
            }
            catch(Exception ex)
            {
                _context?.Log($"NetError: {ex.Message}", LogLevel.Error);
            }
        }

        private volatile bool _isUpdating = false;

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_activeInterface == null || _isUpdating) return;

            _isUpdating = true;

            try
            {
                // Run on background thread to avoid UI freeze if GetIPStatistics is slow
                // Chạy trên luồng nền để tránh đóng băng UI nếu GetIPStatistics chậm
                Task.Run(() => 
                {
                    try 
                    {
                        var stats = _activeInterface.GetIPStatistics();
                        long sent = stats.BytesSent;
                        long received = stats.BytesReceived;

                        long upSpeed = sent - _bytesSent;
                        long downSpeed = received - _bytesReceived;

                        _bytesSent = sent;
                        _bytesReceived = received;

                        // Update UI on Dispatcher / Cập nhật UI trên Dispatcher
                        Application.Current.Dispatcher.Invoke(() => UpdateUI(upSpeed, downSpeed));
                    }
                    catch { }
                    finally
                    {
                        _isUpdating = false;
                    }
                });
            }
            catch 
            {
                _isUpdating = false;
            }
        }

        private void UpdateUI(long up, long down)
        {
            DownloadText.Text = FormatSpeed(down);
            UploadText.Text = FormatSpeed(up);

            // Update Graph Data / Cập nhật dữ liệu đồ thị
            _dlHistory.Add(down);
            _dlHistory.RemoveAt(0);
            _ulHistory.Add(up);
            _ulHistory.RemoveAt(0);

            DrawGraph();
        }

        private void DrawGraph()
        {
            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            // Auto-scale logic / Logic tự động chia tỉ lệ
            double currentMax = Math.Max(_dlHistory.Max(), _ulHistory.Max());
            if (currentMax > _maxSpeed) _maxSpeed = currentMax * 1.2;
            else if (currentMax < _maxSpeed * 0.5 && _maxSpeed > 1024*1024) _maxSpeed *= 0.8;

            double step = width / (MaxPoints - 1);

            // Update Download Line Points / Cập nhật điểm đường Download
            _dlLine.Points.Clear();
            for (int i = 0; i < MaxPoints; i++)
            {
                double y = height - (_dlHistory[i] / _maxSpeed * height);
                _dlLine.Points.Add(new Point(i * step, y));
            }

            // Update Upload Line Points / Cập nhật điểm đường Upload
            _ulLine.Points.Clear();
            for (int i = 0; i < MaxPoints; i++)
            {
                double y = height - (_ulHistory[i] / _maxSpeed * height);
                _ulLine.Points.Add(new Point(i * step, y));
            }
        }

        private string FormatSpeed(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B/s";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB/s";
            return $"{bytes / 1024.0 / 1024.0:F1} MB/s";
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                (sender as Button).Content = "RESUME";
            }
            else
            {
                _timer.Start();
                (sender as Button).Content = "STOP MONITOR";
            }
        }
    }
}
