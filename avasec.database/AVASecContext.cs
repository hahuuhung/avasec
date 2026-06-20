using Microsoft.EntityFrameworkCore;
using AVASec.Core.Models;
using System;
using System.IO;

namespace AVASec.Database
{
    /// <summary>
    /// Entity Framework DbContext for AVA Security / DbContext cho AVA Security
    /// Manages database operations / Quản lý các thao tác cơ sở dữ liệu
    /// </summary>
    public class AVASecContext : DbContext
    {
        public AVASecContext() { }

        public AVASecContext(DbContextOptions<AVASecContext> options) : base(options) { }

        /// <summary>
        /// Users table / Bảng người dùng
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Licenses table / Bảng giấy phép
        /// </summary>
        public DbSet<License> Licenses { get; set; }

        /// <summary>
        /// Scan history table / Bảng lịch sử quét
        /// </summary>
        public DbSet<ScanHistory> ScanHistories { get; set; }

        /// <summary>
        /// Quarantined files table / Bảng file cách ly
        /// </summary>
        public DbSet<QuarantinedFile> QuarantinedFiles { get; set; }

        /// <summary>
        /// Threat cache table / Bảng bộ nhớ đệm mối đe dọa đám mây
        /// </summary>
        public DbSet<ThreatCacheRecord> ThreatCaches { get; set; }

        /// <summary>
        /// Configure database connection / Cấu hình kết nối cơ sở dữ liệu
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Get application data folder / Lấy thư mục dữ liệu ứng dụng
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string sysAntiPath = Path.Combine(appDataPath, BrandConstants.AppDataFolder);
                
                // Create folder if not exists / Tạo thư mục nếu chưa tồn tại
                Directory.CreateDirectory(sysAntiPath);
                
                // Database file path / Đường dẫn file cơ sở dữ liệu
                string dbPath = Path.Combine(sysAntiPath, "avasec.db");
                
                // Configure SQLite / Cấu hình SQLite
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        /// <summary>
        /// Configure model relationships / Cấu hình mối quan hệ model
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - License: One-to-One / Một-Một
            modelBuilder.Entity<User>()
                .HasOne(u => u.License)
                .WithOne(l => l.User)
                .HasForeignKey<License>(l => l.UserId);

            // User - ScanHistory: One-to-Many / Một-Nhiều
            modelBuilder.Entity<User>()
                .HasMany<ScanHistory>()
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);

            // ScanHistory - QuarantinedFiles: One-to-Many / Một-Nhiều
            modelBuilder.Entity<ScanHistory>()
                .HasMany(s => s.QuarantinedFiles)
                .WithOne(q => q.ScanHistory)
                .HasForeignKey(q => q.ScanId);

            // Unique constraints / Ràng buộc duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<License>()
                .HasIndex(l => l.LicenseKey)
                .IsUnique();
        }
    }
}
