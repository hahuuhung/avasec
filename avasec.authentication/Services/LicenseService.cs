using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Database;

namespace AVASec.Authentication.Services
{
    /// <summary>
    /// License service implementation / Triển khai dịch vụ giấy phép
    /// Manages software licensing operations / Quản lý các thao tác giấy phép phần mềm
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly AVASecContext _context;

        public LicenseService(AVASecContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate new license / Tạo giấy phép mới
        /// </summary>
        public async Task<License> GenerateLicenseAsync(int userId, string licenseType)
        {
            // Calculate expiry based on license type / Tính ngày hết hạn dựa trên loại giấy phép
            int durationDays = licenseType switch
            {
                "Trial" => 14,      // 14 days / 14 ngày
                "Monthly" => 30,    // 30 days / 30 ngày
                "Yearly" => 365,    // 365 days / 365 ngày
                "Lifetime" => 36500, // 100 years / 100 năm
                _ => 14
            };

            var license = new License
            {
                UserId = userId,
                LicenseKey = GenerateUniqueLicenseKey(),
                IssueDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(durationDays),
                IsActive = true,
                LicenseType = licenseType
            };

            // Check if user already has a license / Kiểm tra người dùng đã có giấy phép chưa
            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (existingLicense != null)
            {
                // Update existing license / Cập nhật giấy phép hiện có
                existingLicense.LicenseKey = license.LicenseKey;
                existingLicense.IssueDate = license.IssueDate;
                existingLicense.ExpiryDate = license.ExpiryDate;
                existingLicense.IsActive = true;
                existingLicense.LicenseType = licenseType;

                await _context.SaveChangesAsync();
                return existingLicense;
            }
            else
            {
                // Create new license / Tạo giấy phép mới
                _context.Licenses.Add(license);
                await _context.SaveChangesAsync();
                return license;
            }
        }

        /// <summary>
        /// Validate license / Kiểm tra giấy phép
        /// </summary>
        public async Task<bool> ValidateLicenseAsync(int userId)
        {
            var license = await _context.Licenses
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (license == null)
                return false;

            return license.IsValid();
        }

        /// <summary>
        /// Get user license / Lấy giấy phép người dùng
        /// </summary>
        public async Task<License?> GetLicenseAsync(int userId)
        {
            return await _context.Licenses
                .FirstOrDefaultAsync(l => l.UserId == userId);
        }

        /// <summary>
        /// Extend license / Gia hạn giấy phép
        /// </summary>
        public async Task<(bool Success, string Message)> ExtendLicenseAsync(int userId, int additionalDays)
        {
            try
            {
                var license = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.UserId == userId);

                if (license == null)
                {
                    return (false, "License not found. / Không tìm thấy giấy phép.");
                }

                // Extend from current expiry or today (whichever is later) / Gia hạn từ ngày hết hạn hoặc hôm nay
                DateTime baseDate = license.ExpiryDate > DateTime.Now ? license.ExpiryDate : DateTime.Now;
                license.ExpiryDate = baseDate.AddDays(additionalDays);
                license.IsActive = true;

                await _context.SaveChangesAsync();

                return (true, $"License extended until {license.ExpiryDate:yyyy-MM-dd}. / Giấy phép đã được gia hạn đến {license.ExpiryDate:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                return (false, $"Extension failed: {ex.Message} / Gia hạn thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke license / Thu hồi giấy phép
        /// </summary>
        public async Task<(bool Success, string Message)> RevokeLicenseAsync(int userId)
        {
            try
            {
                var license = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.UserId == userId);

                if (license == null)
                {
                    return (false, "License not found. / Không tìm thấy giấy phép.");
                }

                license.IsActive = false;
                await _context.SaveChangesAsync();

                return (true, "License revoked successfully. / Thu hồi giấy phép thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Revocation failed: {ex.Message} / Thu hồi thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate unique license key / Tạo mã giấy phép duy nhất
        /// </summary>
        private string GenerateUniqueLicenseKey()
        {
            string prefix = "SA"; // AVA Security prefix / Tiền tố AVA Security
            string part1 = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            string part2 = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            string part3 = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            string part4 = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

            return $"{prefix}-{part1}-{part2}-{part3}-{part4}";
        }
    }
}
