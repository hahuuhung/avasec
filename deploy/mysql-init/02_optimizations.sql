-- Database Optimizations for SysAnti
-- Created: 2026-02-07 18:25
-- Purpose: Add indexes and optimize queries for better performance

USE avasec_db;

-- ============================================
-- 1. ADD INDEXES FOR PERFORMANCE
-- ============================================

-- Users table indexes
-- Frequently queried by Username and Email in auth operations
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_created_active ON Users(CreatedAt, IsActive);

-- Licenses table indexes
-- Frequently joined with Users and queried by UserId
CREATE INDEX IF NOT EXISTS idx_licenses_userid ON Licenses(UserId);
CREATE INDEX IF NOT EXISTS idx_licenses_type_expiry ON Licenses(LicenseType, ExpiryDate);
CREATE INDEX IF NOT EXISTS idx_licenses_expiry ON Licenses(ExpiryDate);
CREATE INDEX IF NOT EXISTS idx_licenses_key ON Licenses(LicenseKey);

-- ApiKeys table indexes
CREATE INDEX IF NOT EXISTS idx_apikeys_key ON ApiKeys(ApiKey);
CREATE INDEX IF NOT EXISTS idx_apikeys_active ON ApiKeys(IsActive);

-- ============================================
-- 2. ADD NOTIFICATIONS TABLE
-- ============================================

CREATE TABLE IF NOT EXISTS Notifications (
    NotificationId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,  -- NULL means broadcast to all users
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    NotificationType VARCHAR(50) DEFAULT 'info', -- info, warning, success, error
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NULL,
    Priority INT DEFAULT 0, -- 0=normal, 1=high, 2=urgent
    ImageUrl VARCHAR(255) NULL,
    ActionUrl VARCHAR(255) NULL,
    IsPromotional BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_notifications_userid_read ON Notifications(UserId, IsRead);
CREATE INDEX IF NOT EXISTS idx_notifications_created ON Notifications(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_expires ON Notifications(ExpiresAt);

-- ============================================
-- 3. ADD ANALYTICS/STATISTICS TABLES
-- ============================================

-- User activity tracking
CREATE TABLE IF NOT EXISTS UserActivity (
    ActivityId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    ActivityType VARCHAR(50) NOT NULL, -- login, scan, update, etc.
    ActivityDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    IpAddress VARCHAR(45) NULL,
    UserAgent TEXT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_activity_userid_date ON UserActivity(UserId, ActivityDate);
CREATE INDEX IF NOT EXISTS idx_activity_type_date ON UserActivity(ActivityType, ActivityDate);
CREATE INDEX IF NOT EXISTS idx_activity_date ON UserActivity(ActivityDate);

-- License revenue tracking (for analytics)
CREATE TABLE IF NOT EXISTS LicenseTransactions (
    TransactionId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    LicenseType VARCHAR(20) NOT NULL,
    Amount DECIMAL(10, 2) DEFAULT 0.00,
    Currency VARCHAR(10) DEFAULT 'VND',
    TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    PaymentMethod VARCHAR(50) NULL, -- bank, momo, zalo, etc.
    Status VARCHAR(20) DEFAULT 'pending', -- pending, completed, failed
    Notes TEXT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_transactions_userid ON LicenseTransactions(UserId);
CREATE INDEX IF NOT EXISTS idx_transactions_date_status ON LicenseTransactions(TransactionDate, Status);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON LicenseTransactions(LicenseType);

-- ============================================
-- 4. ADD SYSTEM LOGS TABLE
-- ============================================

CREATE TABLE IF NOT EXISTS SystemLogs (
    LogId INT AUTO_INCREMENT PRIMARY KEY,
    LogLevel VARCHAR(20) NOT NULL, -- error, warn, info, debug
    LogMessage TEXT NOT NULL,
    LogStack TEXT NULL,
    Source VARCHAR(100) NULL, -- route or module name
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IpAddress VARCHAR(45) NULL
);

CREATE INDEX IF NOT EXISTS idx_logs_level_date ON SystemLogs(LogLevel, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_logs_date ON SystemLogs(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_logs_source ON SystemLogs(Source);

-- ============================================
-- 5. OPTIMIZE EXISTING QUERIES WITH VIEWS
-- ============================================

-- Create view for users with license info (used frequently in admin panel)
CREATE OR REPLACE VIEW UserLicenseView AS
SELECT 
    u.UserId,
    u.Username,
    u.Email,
    u.CreatedAt AS UserCreatedAt,
    u.IsActive,
    l.LicenseId,
    l.LicenseKey,
    l.LicenseType,
    l.IssueDate,
    l.ExpiryDate,
    l.IsActive AS LicenseActive,
    CASE 
        WHEN l.ExpiryDate IS NULL THEN 0
        WHEN l.ExpiryDate < NOW() THEN 0
        ELSE DATEDIFF(l.ExpiryDate, NOW())
    END AS RemainingDays,
    CASE 
        WHEN l.ExpiryDate < NOW() THEN 'Expired'
        WHEN DATEDIFF(l.ExpiryDate, NOW()) <= 7 THEN 'Expiring Soon'
        ELSE 'Active'
    END AS LicenseStatus
FROM Users u
LEFT JOIN Licenses l ON u.UserId = l.UserId;

-- ============================================
-- 7. SAMPLE DATA FOR TESTING
-- ============================================

-- Insert a sample broadcast notification
INSERT INTO Notifications (UserId, Title, Message, NotificationType, Priority)
VALUES (NULL, 'Welcome to SysAnti', 'Thank you for using AVASec Anti-virus. Stay protected!', 'info', 0);

-- End of optimizations
