using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVASec.Core.Models
{
    /// <summary>
    /// Quarantined file model / Mô hình file cách ly
    /// Stores information about isolated malicious files / Lưu thông tin về file độc hại bị cách ly
    /// </summary>
    public class QuarantinedFile
    {
        /// <summary>
        /// Primary key / Khóa chính
        /// </summary>
        [Key]
        public int FileId { get; set; }

        /// <summary>
        /// Foreign key to ScanHistory / Khóa ngoại đến ScanHistory
        /// </summary>
        [Required]
        public int ScanId { get; set; }

        /// <summary>
        /// Original file path / Đường dẫn file gốc
        /// </summary>
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Threat/virus name / Tên mối đe dọa/virus
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ThreatName { get; set; } = string.Empty;

        /// <summary>
        /// File hash (SHA256) / Hash file (SHA256)
        /// </summary>
        [StringLength(64)]
        public string FileHash { get; set; } = string.Empty;

        /// <summary>
        /// Quarantine timestamp / Thời điểm cách ly
        /// </summary>
        public DateTime QuarantinedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// File size in bytes / Kích thước file (bytes)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Whether file has been restored / File đã được khôi phục chưa
        /// </summary>
        public bool IsRestored { get; set; } = false;

        /// <summary>
        /// Quarantined file location / Vị trí file bị cách ly
        /// </summary>
        [StringLength(500)]
        public string QuarantineLocation { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to ScanHistory / Thuộc tính điều hướng đến ScanHistory
        /// </summary>
        [ForeignKey("ScanId")]
        public ScanHistory? ScanHistory { get; set; }
    }
}
