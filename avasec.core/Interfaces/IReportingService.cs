using System.Collections.Generic;
using System.Threading.Tasks;
using AVASec.Core.Models;

namespace AVASec.Core.Interfaces
{
    /// <summary>
    /// Reporting service interface / Interface dịch vụ báo cáo
    /// Handles PDF generation / Xử lý tạo PDF
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Generate system report PDF / Tạo báo cáo hệ thống PDF
        /// </summary>
        void GenerateSystemReport(string outputPath, List<ScanHistory> scanHistory, List<QuarantinedFile> quarantinedFiles);
    }
}
