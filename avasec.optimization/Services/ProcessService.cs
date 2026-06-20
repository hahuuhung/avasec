using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AVASec.Optimization.Services
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public long MemoryUsage { get; set; } // Bytes
        public string MemoryUsageFormatted => $"{MemoryUsage / 1024 / 1024} MB";
    }

    public interface IProcessService
    {
        List<ProcessInfo> GetRunningProcesses();
        bool KillProcess(int processId);
    }

    public class ProcessService : IProcessService
    {
        public List<ProcessInfo> GetRunningProcesses()
        {
            try
            {
                var processes = Process.GetProcesses();
                return processes.Select(p => {
                    try
                    {
                        return new ProcessInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Title = p.MainWindowTitle,
                            MemoryUsage = p.WorkingSet64
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(p => p != null)
                .OrderByDescending(p => p.MemoryUsage)
                .ToList();
            }
            catch
            {
                return new List<ProcessInfo>();
            }
        }

        public bool KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kill Process Error: {ex.Message}");
                return false;
            }
        }
    }
}
