using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVASec.Core.Models
{
    /// <summary>
    /// Scan history model / Mô hình lịch sử quét
    /// Records virus scan operations / Ghi lại các hoạt động quét virus
    /// </summary>
    public class ScanHistory
    {
        /// <summary>
        /// Primary key / Khóa chính
        /// </summary>
        [Key]
        public int ScanId { get; set; }

        /// <summary>
        /// Foreign key to User / Khóa ngoại đến User
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Scan timestamp / Thời điểm quét
        /// </summary>
        public DateTime ScanDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Number of files scanned / Số file đã quét
        /// </summary>
        public int FilesScanned { get; set; }

        /// <summary>
        /// Number of threats found / Số mối đe dọa tìm thấy
        /// </summary>
        public int ThreatsFound { get; set; }

        /// <summary>
        /// Scan type (quick, full, custom) / Loại quét (nhanh, đầy đủ, tùy chỉnh)
        /// </summary>
        [StringLength(20)]
        public string ScanType { get; set; } = "Quick";

        /// <summary>
        /// Scan duration in seconds / Thời gian quét (giây)
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Scan status (completed, interrupted, failed) / Trạng thái quét
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Completed";

        /// <summary>
        /// Navigation property to User / Thuộc tính điều hướng đến User
        /// </summary>
        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>
        /// Quarantined files from this scan / File bị cách ly từ lần quét này
        /// </summary>
        public ICollection<QuarantinedFile> QuarantinedFiles { get; set; } = new List<QuarantinedFile>();
    }
}
