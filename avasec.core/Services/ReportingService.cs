using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.Core.Services
{
    /// <summary>
    /// Reporting Service implementation using QuestPDF
    /// Triển khai dịch vụ báo cáo sử dụng QuestPDF
    /// </summary>
    public class ReportingService : IReportingService
    {
        public ReportingService()
        {
            // Set license type (Community is free for individuals/open source)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GenerateSystemReport(string outputPath, List<ScanHistory> scanHistory, List<QuarantinedFile> quarantinedFiles)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.SegoeUI));

                    page.Header()
                        .Text($"AVA Security - Security Report / Báo cáo Bảo mật")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            // Info Section / Thông tin chung
                            x.Item().Text(text =>
                            {
                                text.Span("Date / Ngày tạo: ").SemiBold();
                                text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                            });

                            // Scan History Section / Lịch sử quét
                            x.Item().Text("Scan History / Lịch sử Quét").FontSize(16).SemiBold().FontColor(Colors.Grey.Darken2);
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#");
                                    header.Cell().Element(CellStyle).Text("Date/Ngày");
                                    header.Cell().Element(CellStyle).Text("Type/Loại");
                                    header.Cell().Element(CellStyle).Text("Files/Số file");
                                    header.Cell().Element(CellStyle).Text("Threats/Mối đe dọa");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                });

                                foreach (var scan in scanHistory)
                                {
                                    table.Cell().Element(CellStyle).Text(scan.ScanId.ToString());
                                    table.Cell().Element(CellStyle).Text(scan.ScanDate.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(CellStyle).Text(scan.ScanType);
                                    table.Cell().Element(CellStyle).Text(scan.FilesScanned.ToString());
                                    table.Cell().Element(CellStyle).Text(scan.ThreatsFound.ToString());

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                                    }
                                }
                            });

                            // Quarantine Section / Khu vực cách ly
                            if (quarantinedFiles.Count > 0)
                            {
                                x.Item().Text("Quarantined Files / File Cách ly").FontSize(16).SemiBold().FontColor(Colors.Red.Medium);
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Threat Name/Tên");
                                        header.Cell().Element(CellStyle).Text("Original Path/Đường dẫn");
                                        header.Cell().Element(CellStyle).Text("Date/Ngày");
                                    });

                                    foreach (var file in quarantinedFiles)
                                    {
                                        table.Cell().Element(CellStyle).Text(file.ThreatName);
                                        table.Cell().Element(CellStyle).Text(file.FilePath).FontSize(10);
                                        table.Cell().Element(CellStyle).Text(file.QuarantinedAt.ToString("dd/MM/yyyy"));
                                    }
                                });
                            }
                            else
                            {
                                x.Item().Text("No quarantined files. / Không có file cách ly.").Italic().FontColor(Colors.Green.Medium);
                            }

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                            }
                        });


                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf(outputPath);
        }
    }
}
