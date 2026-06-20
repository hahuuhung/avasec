using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVASec.Core.Models
{
    /// <summary>
    /// License model / Mô hình giấy phép
    /// Manages software licensing and expiration / Quản lý giấy phép phần mềm và thời hạn
    /// </summary>
    public class License
    {
        /// <summary>
        /// Primary key / Khóa chính
        /// </summary>
        [Key]
        public int LicenseId { get; set; }

        /// <summary>
        /// Foreign key to User / Khóa ngoại đến User
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Unique license key / Mã giấy phép duy nhất
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LicenseKey { get; set; } = string.Empty;

        /// <summary>
        /// Issue date / Ngày phát hành
        /// </summary>
        public DateTime IssueDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Expiry date / Ngày hết hạn
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Whether license is active / Giấy phép có đang hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// License type (trial, monthly, yearly, lifetime) / Loại giấy phép (dùng thử, tháng, năm, trọn đời)
        /// </summary>
        [StringLength(20)]
        public string LicenseType { get; set; } = "Trial";

        /// <summary>
        /// Check if license is valid / Kiểm tra giấy phép có hợp lệ
        /// </summary>
        public bool IsValid()
        {
            return IsActive && DateTime.Now <= ExpiryDate;
        }

        /// <summary>
        /// Get remaining days / Lấy số ngày còn lại
        /// </summary>
        public int GetRemainingDays()
        {
            if (DateTime.Now > ExpiryDate)
                return 0;

            return (ExpiryDate - DateTime.Now).Days;
        }

        /// <summary>
        /// Navigation property to User / Thuộc tính điều hướng đến User
        /// </summary>
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
