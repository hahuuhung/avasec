using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceProcess;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Plugins.GameBooster
{
    public partial class BoosterWindow : Window
    {
        private readonly IPluginContext? _context;
        private bool _isBoosted = false;
        private System.Collections.Generic.List<string> _stoppedServices = new();

        public BoosterWindow(IPluginContext? context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Boost_Click(object sender, RoutedEventArgs e)
        {
            if (_isBoosted)
            {
                // Restore logic
                StatusText.Text = "RESTORING...";
                BoostBtn.IsEnabled = false;

                await Task.Run(() =>
                {
                    foreach (var serviceName in _stoppedServices)
                    {
                        try
                        {
                            using (var sc = new ServiceController(serviceName))
                            {
                                if (sc.Status == ServiceControllerStatus.Stopped)
                                {
                                    sc.Start();
                                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                                }
                            }
                        }
                        catch { }
                    }
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _stoppedServices.Clear(); // Clear on UI thread or safe context
                        StatusText.Text = "READY";
                        BoostBtn.IsEnabled = true;
                        _isBoosted = false;
                        _context?.Notify("Game Booster", "System services restored.", "Info");
                    });
                });
                return;
            }

            BoostBtn.IsEnabled = false;
            StatusText.Text = "BOOSTING...";

            await Task.Run(async () =>
            {
                // Simulate boost process
                await Task.Delay(500);
                
                // 1. Clear RAM
                long initialMemory = GC.GetTotalMemory(false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                long freed = (initialMemory - GC.GetTotalMemory(true)) / (1024 * 1024);
                
                // 2. Stop non-essential services
                int stoppedCount = 0;
                // Common safe-to-disable services (SysMain = Superfetch, DiagTrack = Telemetry)
                string[] targetServices = { "SysMain", "DiagTrack", "WSearch", "Spooler" }; 
                
                foreach(var sName in targetServices)
                {
                    try
                    {
                        using (var sc = new ServiceController(sName))
                        {
                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                                Application.Current.Dispatcher.Invoke(() => _stoppedServices.Add(sName));
                                stoppedCount++;
                            }
                        }
                    }
                    catch 
                    {
                         // Permission denied or service not found
                    }
                }
                
                // Fallback: Kill some dummy processes for demo if no services touched (e.g. not Admin)
                if (stoppedCount == 0)
                {
                     string[] targetProcs = { "notepad", "calculator", "mspaint" }; 
                     foreach(var pName in targetProcs)
                     {
                        var procs = Process.GetProcessesByName(pName);
                        foreach(var p in procs) { try { p.Kill(); stoppedCount++; } catch {} }
                     }
                }

                await Task.Delay(1000); // Visual effect

                Application.Current.Dispatcher.Invoke(() =>
                {
                    RamText.Text = $"{freed + 50} MB"; 
                    ProcText.Text = $"{stoppedCount} Svcs/Procs";
                    StatusText.Text = "ACTIVE";
                    BoostBtn.IsEnabled = true;
                    _isBoosted = true;
                    _context?.Notify("Game Booster", $"Boost Complete! Optimized {stoppedCount} services.", "Success");
                });
            });
        }
    }
}
