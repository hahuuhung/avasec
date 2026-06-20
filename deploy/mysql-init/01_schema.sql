CREATE DATABASE IF NOT EXISTS avasec_db;
USE avasec_db;

-- Users Table
CREATE TABLE IF NOT EXISTS Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

-- Licenses Table
CREATE TABLE IF NOT EXISTS Licenses (
    LicenseId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    LicenseKey VARCHAR(100) NOT NULL UNIQUE,
    IssueDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate DATETIME NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    LicenseType VARCHAR(20) DEFAULT 'Trial',
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- ApiKeys Table
CREATE TABLE IF NOT EXISTS ApiKeys (
    KeyId INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(100) NOT NULL,
    ApiKey VARCHAR(255) NOT NULL UNIQUE,
    CreatedBy INT, -- Optional link to User
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId) ON DELETE SET NULL
);

-- Sample Data (Admin User)
-- Password is 'admin123' (plain text for demo to match auth.js logic)
INSERT IGNORE INTO Users (Username, PasswordHash, Email, IsActive) 
VALUES ('admin', 'admin123', 'hhhunghhh@gmail.com', TRUE);

-- Sample Data (Api Key)
INSERT IGNORE INTO ApiKeys (Title, ApiKey, IsActive)
VALUES ('Default Web Client', 'sk_live_sysanti_default_key_12345', TRUE);
