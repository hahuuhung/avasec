using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AVASec.Core.Models;
using AVASec.Database;

namespace AVASec.Antivirus.Services
{
    /// <summary>
    /// Quarantine service / Dịch vụ cách ly
    /// Manages quarantined files / Quản lý file bị cách ly
    /// </summary>
    public class QuarantineService
    {
        private readonly AVASecContext _context;
        private readonly string _quarantinePath;

        public QuarantineService(AVASecContext context)
        {
            _context = context;
            
            // Set quarantine folder / Đặt thư mục cách ly
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _quarantinePath = Path.Combine(appData, "avasec", "Quarantine");
            
            // Create quarantine folder if not exists / Tạo thư mục cách ly nếu chưa tồn tại
            Directory.CreateDirectory(_quarantinePath);
        }

        /// <summary>
        /// Quarantine a file / Cách ly một file
        /// </summary>
        public async Task<(bool Success, string Message)> QuarantineFileAsync(string filePath, string threatName, int scanId)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, "File not found / File không tồn tại");
                }

                var fileInfo = new FileInfo(filePath);
                
                // Generate unique quarantine filename / Tạo tên file cách ly duy nhất
                string quarantineFileName = $"{Guid.NewGuid()}_{Path.GetFileName(filePath)}";
                string quarantineFilePath = Path.Combine(_quarantinePath, quarantineFileName);

                // Move file to quarantine / Di chuyển file vào khu cách ly
                File.Move(filePath, quarantineFilePath);

                // Add to database / Thêm vào cơ sở dữ liệu
                var quarantinedFile = new QuarantinedFile
                {
                    ScanId = scanId,
                    FilePath = filePath,
                    ThreatName = threatName,
                    FileSize = fileInfo.Length,
                    QuarantinedAt = DateTime.Now,
                    QuarantineLocation = quarantineFilePath,
                    IsRestored = false
                };

                _context.QuarantinedFiles.Add(quarantinedFile);
                await _context.SaveChangesAsync();

                return (true, $"File quarantined successfully / File đã được cách ly thành công: {quarantineFilePath}");
            }
            catch (Exception ex)
            {
                return (false, $"Quarantine failed / Cách ly thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all quarantined files / Lấy tất cả file đã cách ly
        /// </summary>
        public async Task<List<QuarantinedFile>> GetQuarantinedFilesAsync()
        {
            return await _context.QuarantinedFiles
                .Where(q => !q.IsRestored)
                .OrderByDescending(q => q.QuarantinedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Restore a quarantined file / Khôi phục file đã cách ly
        /// </summary>
        public async Task<(bool Success, string Message)> RestoreFileAsync(int fileId)
        {
            try
            {
                var quarantinedFile = await _context.QuarantinedFiles.FindAsync(fileId);

                if (quarantinedFile == null)
                {
                    return (false, "Quarantined file not found / Không tìm thấy file cách ly");
                }

                if (quarantinedFile.IsRestored)
                {
                    return (false, "File already restored / File đã được khôi phục");
                }

                if (!File.Exists(quarantinedFile.QuarantineLocation))
                {
                    return (false, "Quarantined file not found on disk / Không tìm thấy file cách ly trên đĩa");
                }

                // Restore to original location / Khôi phục về vị trí ban đầu
                string originalDir = Path.GetDirectoryName(quarantinedFile.FilePath)!;
                
                // Create directory if not exists / Tạo thư mục nếu chưa tồn tại
                Directory.CreateDirectory(originalDir);

                // Move file back / Di chuyển file trở lại
                File.Move(quarantinedFile.QuarantineLocation, quarantinedFile.FilePath);

                // Update database / Cập nhật cơ sở dữ liệu
                quarantinedFile.IsRestored = true;
                await _context.SaveChangesAsync();

                return (true, $"File restored successfully / File đã được khôi phục thành công: {quarantinedFile.FilePath}");
            }
            catch (Exception ex)
            {
                return (false, $"Restore failed / Khôi phục thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a quarantined file permanently / Xóa vĩnh viễn file cách ly
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteFileAsync(int fileId)
        {
            try
            {
                var quarantinedFile = await _context.QuarantinedFiles.FindAsync(fileId);

                if (quarantinedFile == null)
                {
                    return (false, "Quarantined file not found / Không tìm thấy file cách ly");
                }

                // Delete from disk / Xóa khỏi đĩa
                if (File.Exists(quarantinedFile.QuarantineLocation))
                {
                    File.Delete(quarantinedFile.QuarantineLocation);
                }

                // Remove from database / Xóa khỏi cơ sở dữ liệu
                _context.QuarantinedFiles.Remove(quarantinedFile);
                await _context.SaveChangesAsync();

                return (true, "File deleted permanently / File đã được xóa vĩnh viễn");
            }
            catch (Exception ex)
            {
                return (false, $"Delete failed / Xóa thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all quarantined files / Xóa tất cả file cách ly
        /// </summary>
        public async Task<(bool Success, string Message, int FilesDeleted)> ClearAllAsync()
        {
            try
            {
                var quarantinedFiles = await GetQuarantinedFilesAsync();
                int deletedCount = 0;

                foreach (var file in quarantinedFiles)
                {
                    var result = await DeleteFileAsync(file.FileId);
                    if (result.Success)
                        deletedCount++;
                }

                return (true, $"Cleared {deletedCount} files / Đã xóa {deletedCount} file", deletedCount);
            }
            catch (Exception ex)
            {
                return (false, $"Clear all failed / Xóa tất cả thất bại: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Get quarantine folder size / Lấy kích thước thư mục cách ly
        /// </summary>
        public long GetQuarantineFolderSize()
        {
            try
            {
                if (!Directory.Exists(_quarantinePath))
                    return 0;

                return Directory.GetFiles(_quarantinePath, "*", SearchOption.TopDirectoryOnly)
                    .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get recent scan history / Lấy lịch sử quét gần đây
        /// </summary>
        public async Task<List<ScanHistory>> GetRecentScanHistoryAsync(int count = 10)
        {
            return await _context.ScanHistories
                .OrderByDescending(s => s.ScanDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
