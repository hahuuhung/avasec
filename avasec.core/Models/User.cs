using System;
using System.ComponentModel.DataAnnotations;

namespace AVASec.Core.Models
{
    /// <summary>
    /// User model / Mô hình người dùng
    /// Represents a user account in the system / Đại diện cho tài khoản người dùng trong hệ thống
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key / Khóa chính
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Username for login / Tên đăng nhập
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password / Mật khẩu đã mã hóa
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User email address / Địa chỉ email người dùng
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Account creation timestamp / Thời điểm tạo tài khoản
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether account is active / Tài khoản có đang hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Associated license / Giấy phép liên kết
        /// </summary>
        public License? License { get; set; }
    }
}
