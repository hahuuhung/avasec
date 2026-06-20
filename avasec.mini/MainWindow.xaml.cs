using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using AVASec.Mini.Services;
using Application = System.Windows.Application;

namespace AVASec.Mini;

public partial class MainWindow : Window
{
    private readonly MiniMonitorService _monitor = new();
    private readonly MiniBoostService _boost = new();
    private readonly MiniCleanService _clean = new();
    private readonly DispatcherTimer _timer;
    private NotifyIcon? _trayIcon;
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => RefreshMetrics();
        Loaded += OnLoaded;
        Closing += (_, e) =>
        {
            if (_trayIcon?.Visible == true)
            {
                e.Cancel = true;
                Hide();
            }
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionNearTaskbar();
        SetupTray();
        SyncFontScaleUi();
        RefreshDailyQuote();
        RefreshCalendar();
        AppendLog("Sẵn sàng — chúc bạn một ngày vui khỏe / Ready — have a healthy day.");
        RefreshMetrics();
        await RefreshWeatherAsync();
        await Task.Delay(500);
        RefreshMetrics();
        _timer.Start();
    }


    private void PositionNearTaskbar()
    {
        var area = SystemParameters.WorkArea;
        UpdateLayout();
        var w = ActualWidth > 0 ? ActualWidth : Width;
        var h = ActualHeight > 0 ? ActualHeight : Height;
        Left = area.Left + (area.Width - w) / 2;
        Top = area.Bottom - h - 4;
    }

    private void SyncFontScaleUi()
    {
        FontScaleLabel.Text = MiniFontScaleService.FormatLabel(MiniFontScaleService.CurrentScale);
        FontDecreaseButton.IsEnabled = MiniFontScaleService.CurrentScale > MiniFontScaleService.MinScale + 0.001;
        FontIncreaseButton.IsEnabled = MiniFontScaleService.CurrentScale < MiniFontScaleService.MaxScale - 0.001;
    }

    private void ApplyFontScale(double scale)
    {
        MiniFontScaleService.ApplyToResources(Application.Current.Resources, scale);
        MiniFontScaleService.Save(scale);
        SyncFontScaleUi();
    }

    private void FontDecreaseButton_Click(object sender, RoutedEventArgs e) =>
        ApplyFontScale(MiniFontScaleService.CurrentScale - MiniFontScaleService.Step);

    private void FontIncreaseButton_Click(object sender, RoutedEventArgs e) =>
        ApplyFontScale(MiniFontScaleService.CurrentScale + MiniFontScaleService.Step);

    private void SetupTray()
    {
        _trayIcon = new NotifyIcon { Text = "AVA Security Mini", Icon = SystemIcons.Shield, Visible = true };
        var menu = new ContextMenuStrip();
        menu.Items.Add("Mở / Open", null, (_, _) => ShowFromTray());
        menu.Items.Add("Thoát / Exit", null, (_, _) =>
        {
            _trayIcon.Visible = false;
            Application.Current.Shutdown();
        });
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    private void RefreshDailyQuote()
    {
        var q = MiniQuoteService.GetToday();
        QuoteText.Text = q.TextVi + " / " + q.TextEn;
        QuoteAuthorText.Text = "— " + q.AuthorVi + " / " + q.AuthorEn;
    }

    private async Task RefreshWeatherAsync()
    {
        try
        {
            ApplyWeather(await MiniWeatherService.GetTodayAsync());
        }
        catch
        {
            ApplyWeather(MiniWeatherService.GetOfflineFallback());
        }
    }

    private void ApplyWeather(WeatherInfo w)
    {
        WeatherIcon.SetKind(w.Kind);
        WeatherLineText.Text = w.LineVi + " / " + w.LineEn;
        WeatherSubText.Text = w.SummaryVi + " · " + w.Temperature + " / " + w.SummaryEn;
    }

    private void RefreshCalendar()
    {
        try
        {
            var c = MiniLunarCalendarService.GetToday();
            SolarDateText.Text = c.SolarLineVi + " / " + c.SolarLineEn;
            LunarDateText.Text = c.LunarLineVi + " / " + c.LunarLineEn;
            CanChiText.Text = c.DayCanChiVi + " · " + c.MonthCanChiVi;
            FestivalText.Text = c.FestivalVi + " / " + c.FestivalEn;
        }
        catch (Exception ex)
        {
            AppendLog("Lịch lỗi / Calendar error: " + ex.Message);
        }
    }

    private void RefreshMetrics()
    {
        try
        {
            var s = _monitor.Capture();
            CpuBar.Value = s.CpuPercent;
            RamBar.Value = s.RamPercent;
            DiskBar.Value = s.DiskPercent;

            CpuText.Text = $"{s.CpuPercent:0}%";
            CpuSubText.Text = s.CpuPercent >= 80 ? "Tải cao / High load" : "Đang sử dụng / In use";

            RamPercentText.Text = $"{s.RamPercent:0}%";
            RamText.Text = s.RamTotalMb > 0
                ? $"{s.RamUsedMb:0} / {s.RamTotalMb:0} MB"
                : "Đang đọc / Reading...";

            DiskPercentText.Text = $"{s.DiskPercent:0}%";
            DiskText.Text = s.DiskFreeGb > 0
                ? $"{s.DiskFreeGb:0.#} GB trống / free"
                : "Đang đọc / Reading...";

            TipText.Text = MiniTipService.GetTip(s);
        }
        catch (Exception ex)
        {
            AppendLog("Giám sát lỗi / Monitor error: " + ex.Message);
        }
    }

    private async void BoostButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            var r = await _boost.QuickBoostAsync(new Progress<string>(AppendLog));
            AppendLog(r.MessageVi + " / " + r.MessageEn);
            RefreshMetrics();
        }
        catch (Exception ex) { AppendLog("Lỗi / Error: " + ex.Message); }
        finally { SetBusy(false); }
    }

    private async void CleanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            long b = await _clean.CleanJunkAsync(new Progress<string>(AppendLog));
            AppendLog("Đã dọn / Cleaned: " + MiniCleanService.FormatBytes(b));
            RefreshMetrics();
        }
        catch (Exception ex) { AppendLog("Lỗi / Error: " + ex.Message); }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        BoostButton.IsEnabled = CleanButton.IsEnabled = !busy;
        ActionProgress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        ActionProgress.IsIndeterminate = busy;
    }

    private void AppendLog(string line) =>
        LogText.Text = string.IsNullOrEmpty(LogText.Text) ? line : line + Environment.NewLine + LogText.Text;

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _trayIcon!.Visible = false;
        Close();
    }

    private void TrayButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        _trayIcon?.ShowBalloonTip(1500, "AVA Security Mini", "Chạy nền / Tray mode", ToolTipIcon.Info);
    }

    private async void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        PositionNearTaskbar();
        SyncFontScaleUi();
        RefreshDailyQuote();
        RefreshCalendar();
        RefreshMetrics();
        await RefreshWeatherAsync();
    }

    private void AlwaysOnTopCheck_Changed(object sender, RoutedEventArgs e) =>
        Topmost = AlwaysOnTopCheck.IsChecked == true;

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        _monitor.Dispose();
        _trayIcon?.Dispose();
        base.OnClosed(e);
    }
}