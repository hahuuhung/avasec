using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Diagnostics;

namespace AVASec.Optimization.Services
{
    public interface ISystemTweaksService
    {
        bool FixNetworkPrinterError();
        bool SetWindowsUpdateState(bool enable);
        bool IsWindowsUpdateEnabled();
        bool OptimizeNetwork();
        bool DisableVisualEffects();
        bool DisableTelemetry();
        bool DisableSleep();
        bool DisableHibernation();
        bool SetHighPerformancePlan();
    }

    public class SystemTweaksService : ISystemTweaksService
    {
        public bool FixNetworkPrinterError()
        {
            try
            {
                // Fix for error 0x0000011b (Windows 10/11 shared printer issue)
                // Registry: HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Print
                // Value: RpcAuthnLevelPrivacyEnabled = 0 (DWORD)
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Print", true))
                {
                    if (key != null)
                    {
                        key.SetValue("RpcAuthnLevelPrivacyEnabled", 0, RegistryValueKind.DWord);
                        // Restart Spooler service to apply
                        RestartService("Spooler");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Printer Fix Error: {ex.Message}");
                return false;
            }
        }

        public bool SetWindowsUpdateState(bool enable)
        {
            try
            {
                // Service: wuauserv
                string serviceName = "wuauserv";
                ServiceController sc = new ServiceController(serviceName);
                
                if (enable)
                {
                    // Set to Auto and Start
                    RunCommand($"sc config {serviceName} start= auto");
                    if (sc.Status != ServiceControllerStatus.Running) sc.Start();
                }
                else
                {
                    // Stop and Set to Disabled
                    if (sc.Status == ServiceControllerStatus.Running) sc.Stop();
                    RunCommand($"sc config {serviceName} start= disabled");
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Windows Update Toggle Error: {ex.Message}");
                return false;
            }
        }

        public bool IsWindowsUpdateEnabled()
        {
            try
            {
                ServiceController sc = new ServiceController("wuauserv");
                return sc.StartType != ServiceStartMode.Disabled;
            }
            catch
            {
                return true; // Default assume enabled if can't read
            }
        }

        public bool OptimizeNetwork()
        {
            try
            {
                // Disable autotuning for some, enable 'normal' for most. 'experimental' for speed.
                RunCommand("netsh int tcp set global autotuninglevel=normal");
                RunCommand("netsh int tcp set heuristics disabled");
                RunCommand("ipconfig /flushdns");
                return true;
            }
            catch { return false; }
        }

        public bool DisableVisualEffects()
        {
            try
            {
                // Set VisualFXSetting to 2 (Adjust for best performance)
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"))
                {
                    key.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord);
                }
                return true;
            }
            catch { return false; }
        }

        public bool DisableTelemetry()
        {
            try
            {
                // Disable DataCollection
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    key.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);
                }
                
                // Disable DiagTrack service
                RunCommand("sc stop DiagTrack");
                RunCommand("sc config DiagTrack start= disabled");
                
                return true;
            }
            catch { return false; }
        }

        public bool DisableSleep()
        {
            try
            {
                // Disable sleep on AC and DC
                RunCommand("powercfg /x -timeout-ac-sleep-0");
                RunCommand("powercfg /x -timeout-dc-sleep-0");
                // Disable standby
                RunCommand("powercfg /x -timeout-ac-standby-0");
                RunCommand("powercfg /x -timeout-dc-standby-0");
                return true;
            }
            catch { return false; }
        }

        public bool DisableHibernation()
        {
            try
            {
                RunCommand("powercfg -h off");
                return true;
            }
            catch { return false; }
        }

        public bool SetHighPerformancePlan()
        {
            try
            {
                // High Performance Guid: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
                // If it doesn't exist, we can try to find it or just use the GUID
                RunCommand("powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
                return true;
            }
            catch { return false; }
        }

        private void RestartService(string serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }
                sc.Start();
            }
            catch { }
        }

        private void RunCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.Verb = "runas"; // Ensure admin
            Process.Start(psi)?.WaitForExit();
        }
    }
}
