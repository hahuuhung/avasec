using System;
using System.ComponentModel.DataAnnotations;

namespace AVASec.Core.Models
{
    /// <summary>
    /// Threat Cache Record / Bản ghi bộ nhớ đệm mối đe dọa đám mây
    /// Stores the online threat scanning results for a specific file hash locally
    /// </summary>
    public class ThreatCacheRecord
    {
        /// <summary>
        /// SHA-256 file hash as Primary Key / Mã hash SHA-256 làm Khóa chính
        /// </summary>
        [Key]
        [Required]
        [StringLength(64)]
        public string FileHash { get; set; } = string.Empty;

        /// <summary>
        /// Whether the file is reported as malicious / File có chứa mã độc hay không
        /// </summary>
        public bool IsMalicious { get; set; }

        /// <summary>
        /// Detailed information or engine count / Chi tiết hoặc số lượng engine báo cáo độc hại
        /// </summary>
        [StringLength(256)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when scanned / Thời điểm thực hiện quét
        /// </summary>
        public DateTime ScannedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// The timestamp when the cache expires / Thời điểm hết hạn của cache
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
