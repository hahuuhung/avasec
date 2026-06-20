USE avasec_db;

-- 3. Create ChatMessages Table (Inferred)
CREATE TABLE IF NOT EXISTS ChatMessages (
    MessageId INT AUTO_INCREMENT PRIMARY KEY,
    SessionId VARCHAR(100) NOT NULL,
    UserId VARCHAR(100) NULL, -- Can be 'anonymous' or user ID
    UserName VARCHAR(100) DEFAULT 'Guest',
    Message TEXT,
    IsAgent BOOLEAN DEFAULT FALSE,
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Index for ChatMessages
CREATE INDEX IF NOT EXISTS idx_chat_session ON ChatMessages(SessionId);
CREATE INDEX IF NOT EXISTS idx_chat_created ON ChatMessages(CreatedAt);
