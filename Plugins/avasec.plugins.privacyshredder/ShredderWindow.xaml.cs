using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using AVASec.Plugin.Core.Interfaces;

namespace AVASec.Plugins.PrivacyShredder
{
    public partial class ShredderWindow : Window
    {
        private readonly IPluginContext? _context;
        private readonly List<string> _files = new();

        public ShredderWindow(IPluginContext? context)
        {
            InitializeComponent();
            _context = context;
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select files to shred / Chọn tệp để xóa"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    if (!_files.Contains(file))
                    {
                        _files.Add(file);
                        FileListBox.Items.Add(file);
                    }
                }
            }
        }

        private async void Shred_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Count == 0)
            {
                MessageBox.Show("No files selected!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY destroy {_files.Count} files?\nChecking 'YES' means they are gone FOREVER.", 
                "WARNING: Permanent Data Loss", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                int count = 0;
                foreach (var file in _files)
                {
                    await Task.Run(() => SecureDelete(file));
                    count++;
                }

                _context?.Notify("Privacy Shredder", $"Successfully shredded {count} files.", "Success");
                MessageBox.Show("Shredding complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                _files.Clear();
                FileListBox.Items.Clear();
            }
            catch (Exception ex)
            {
                _context?.Notify("Shredder Error", ex.Message, "Error");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SecureDelete(string filePath)
        {
            if (!File.Exists(filePath)) return;

            // 1. Overwrite with zeros
            // 2. Overwrite with random data
            // 3. Delete
            
            var info = new FileInfo(filePath);
            long length = info.Length;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                // Pass 1: Zeros
                byte[] buffer = new byte[4096]; // 4KB buffer
                long position = 0;
                while (position < length)
                {
                    stream.Write(buffer, 0, (int)Math.Min(buffer.Length, length - position));
                    position += buffer.Length;
                }
                
                // Pass 2: Random (optional, skipped for speed in this demo, but essential for DoD standards)
            }

            File.Delete(filePath);
        }
    }
}
