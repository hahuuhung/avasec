using System;

namespace AVASec.Core.Interfaces
{
    /// <summary>
    /// Authentication service interface / Interface dịch vụ xác thực
    /// Defines authentication operations / Định nghĩa các thao tác xác thực
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Register a new user / Đăng ký người dùng mới
        /// </summary>
        /// <param name="username">Username / Tên đăng nhập</param>
        /// <param name="password">Password / Mật khẩu</param>
        /// <param name="email">Email address / Địa chỉ email</param>
        /// <returns>True if successful / True nếu thành công</returns>
        Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email);

        /// <summary>
        /// Login with credentials / Đăng nhập bằng thông tin xác thực
        /// </summary>
        /// <param name="username">Username / Tên đăng nhập</param>
        /// <param name="password">Password / Mật khẩu</param>
        /// <returns>User ID if successful, null if failed / ID người dùng nếu thành công, null nếu thất bại</returns>
        Task<(bool Success, int? UserId, string Message)> LoginAsync(string username, string password);

        /// <summary>
        /// Get user by ID / Lấy thông tin người dùng theo ID
        /// </summary>
        Task<AVASec.Core.Models.User?> GetUserAsync(int userId);

        /// <summary>
        /// Change user password / Đổi mật khẩu người dùng
        /// </summary>
        /// <param name="userId">User ID / ID người dùng</param>
        /// <param name="oldPassword">Old password / Mật khẩu cũ</param>
        /// <param name="newPassword">New password / Mật khẩu mới</param>
        /// <returns>True if successful / True nếu thành công</returns>
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
