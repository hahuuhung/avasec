using System;
using System.Collections.Generic;

namespace AVASec.Antivirus.Models
{
    /// <summary>
    /// Virus database model / Mô hình cơ sở dữ liệu virus
    /// Structure for virus signatures JSON file / Cấu trúc cho file JSON chữ ký virus
    /// </summary>
    public class VirusDatabase
    {
        /// <summary>
        /// Database version / Phiên bản cơ sở dữ liệu
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Last update timestamp / Thời điểm cập nhật cuối
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// List of virus signatures / Danh sách chữ ký virus
        /// </summary>
        public List<SignatureEntry> Signatures { get; set; } = new List<SignatureEntry>();

        /// <summary>
        /// List of known malware hashes / Danh sách hash malware đã biết
        /// </summary>
        public List<MalwareHashEntry> MalwareHashes { get; set; } = new List<MalwareHashEntry>();
    }

    /// <summary>
    /// Virus signature entry / Mục chữ ký virus
    /// </summary>
    public class SignatureEntry
    {
        /// <summary>
        /// Virus/malware name / Tên virus/malware
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Base64 encoded pattern / Pattern mã hóa Base64
        /// </summary>
        public string Pattern { get; set; } = string.Empty;

        /// <summary>
        /// Threat severity (Low, Medium, High, Critical) / Mức độ nguy hiểm
        /// </summary>
        public string Severity { get; set; } = "Medium";

        /// <summary>
        /// Description / Mô tả
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Malware hash entry / Mục hash malware
    /// </summary>
    public class MalwareHashEntry
    {
        /// <summary>
        /// File hash (SHA256) / Hash file (SHA256)
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Malware name / Tên malware
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Threat severity / Mức độ nguy hiểm
        /// </summary>
        public string Severity { get; set; } = "High";
    }
}
