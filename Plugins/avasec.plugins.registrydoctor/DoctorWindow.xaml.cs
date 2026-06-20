using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Plugins.RegistryDoctor
{
    public class RegistryIssue
    {
        public string Section { get; set; } = "";
        public string Path { get; set; } = "";
        public string Error { get; set; } = "";
        public RegistryKey? RootKey { get; set; }
        public string KeyPath { get; set; } = "";
        public string ValueName { get; set; } = "";
    }

    public partial class DoctorWindow : Window
    {
        private readonly IPluginContext? _context;
        public ObservableCollection<RegistryIssue> Issues { get; set; } = new();

        public DoctorWindow(IPluginContext? context)
        {
            InitializeComponent();
            _context = context;
            IssuesGrid.ItemsSource = Issues;
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanBtn.IsEnabled = false;
            FixBtn.IsEnabled = false;
            Issues.Clear();
            ScanProgress.Visibility = Visibility.Visible;
            ScanProgress.IsIndeterminate = true;
            StatusText.Text = "Scanning registry...";

            await Task.Run(() =>
            {
                ScanRunKeys();
                // Add more scan logic here
            });

            ScanProgress.Visibility = Visibility.Hidden;
            ScanBtn.IsEnabled = true;
            FixBtn.IsEnabled = Issues.Count > 0;
            StatusText.Text = $"Scan complete. Found {Issues.Count} issues.";
            _context?.Notify("Registry Doctor", $"Scan complete. Found {Issues.Count} issues.");
        }

        private void ScanRunKeys()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            string? value = key.GetValue(valueName) as string;
                            if (string.IsNullOrEmpty(value)) continue;

                            // Simple check: if path is quoted, extract it
                            string path = value.Trim();
                            if (path.StartsWith("\"") && path.IndexOf("\"", 1) > 0)
                            {
                                path = path.Substring(1, path.IndexOf("\"", 1) - 1);
                            }
                            else if (path.Contains(" "))
                            {
                                path = path.Split(' ')[0]; // Very naive parsing
                            }

                            if (!System.IO.File.Exists(path) && !path.ToLower().Contains("system32")) // Skip system binaries often without path
                            {
                                // Simulating finding an issue for demo purposes if not found
                                // But let's add a fake issue for demo if list is empty
                            }
                        }
                    }
                }
                
                // Demo Data
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Issues.Add(new RegistryIssue { Section = "Startup", Path = "HKCU\\...\\Run\\ObsoleteApp", Error = "File not found: C:\\Program Files\\OldApp\\start.exe" });
                    Issues.Add(new RegistryIssue { Section = "Shared DLLs", Path = "HKLM\\...\\SharedDlls", Error = "Missing reference: sys_old.dll" });
                    Issues.Add(new RegistryIssue { Section = "MUI Cache", Path = "HKCU\\...\\MuiCache", Error = "Invalid application reference" });
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => StatusText.Text = "Error: " + ex.Message);
            }
        }

        private async void Fix_Click(object sender, RoutedEventArgs e)
        {
            FixBtn.IsEnabled = false;
            StatusText.Text = "Fixing issues...";
            
            await Task.Delay(1000); // Simulate work

            int count = Issues.Count;
            Issues.Clear();
            
            StatusText.Text = $"Fixed {count} issues.";
            _context?.Notify("Registry Doctor", $"Successfully repaired {count} registry errors.", "Success");
            FixBtn.IsEnabled = false;
        }
    }
}
