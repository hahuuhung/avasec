using System;
using AVASec.Core.Models;

namespace AVASec.Core.Interfaces
{
    /// <summary>
    /// License service interface / Interface dịch vụ giấy phép
    /// Manages software licensing / Quản lý giấy phép phần mềm
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Generate a new license key / Tạo mã giấy phép mới
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <param name="licenseType">License type / Loại giấy phép</param>
        /// <returns>License object / Đối tượng giấy phép</returns>
        Task<License> GenerateLicenseAsync(int userId, string licenseType);

        /// <summary>
        /// Validate a license / Kiểm tra giấy phép
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <returns>True if valid / True nếu hợp lệ</returns>
        Task<bool> ValidateLicenseAsync(int userId);

        /// <summary>
        /// Get user license / Lấy giấy phép người dùng
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <returns>License or null / Giấy phép hoặc null</returns>
        Task<License?> GetLicenseAsync(int userId);

        /// <summary>
        /// Extend license period / Gia hạn giấy phép
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <param name="additionalDays">Days to add / Số ngày thêm</param>
        /// <returns>True if successful / True nếu thành công</returns>
        Task<(bool Success, string Message)> ExtendLicenseAsync(int userId, int additionalDays);

        /// <summary>
        /// Revoke/deactivate license / Thu hồi/vô hiệu hóa giấy phép
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <returns>True if successful / True nếu thành công</returns>
        Task<(bool Success, string Message)> RevokeLicenseAsync(int userId);
    }
}
