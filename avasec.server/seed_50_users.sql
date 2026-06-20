USE avasec_db;

-- Xóa dữ liệu cũ nếu muốn reset (Cảnh báo: Xóa sạch dữ liệu test cũ)
-- DELETE FROM Users WHERE Username LIKE 'testuser%';

-- Thêm 50 user và license tương ứng bằng Procedure để nhanh hơn
DELIMITER //

CREATE PROCEDURE IF NOT EXISTS SeedUsers()
BEGIN
    DECLARE i INT DEFAULT 1;
    DECLARE type_var VARCHAR(20);
    DECLARE days_var INT;
    DECLARE email_var VARCHAR(100);
    DECLARE user_var VARCHAR(50);
    DECLARE new_user_id INT;
    
    WHILE i <= 50 DO
        SET user_var = CONCAT('testuser', i);
        SET email_var = CONCAT('test', i, '@sysanti.demo');
        
        -- Chọn loại license ngẫu nhiên
        CASE 
            WHEN i % 3 = 0 THEN SET type_var = 'Premium'; SET days_var = 365;
            WHEN i % 3 = 1 THEN SET type_var = 'Pro'; SET days_var = 30;
            ELSE SET type_var = 'Trial'; SET days_var = 14;
        END CASE;

        -- Insert User
        INSERT IGNORE INTO Users (Username, PasswordHash, Email, IsActive)
        VALUES (user_var, '$2b$10$rDp28sEUnzK5oX/7F.hBvO02N6p6P9e9qN6q', email_var, TRUE);
        
        SET new_user_id = LAST_INSERT_ID();
        
        -- Nếu insert thành công (không bị trùng), thêm license
        IF new_user_id > 0 THEN
            INSERT IGNORE INTO Licenses (UserId, LicenseKey, ExpiryDate, LicenseType, IsActive)
            VALUES (
                new_user_id, 
                CONCAT('SK-', UPPER(SUBSTRING(MD5(RAND()), 1, 12))), 
                DATE_ADD(NOW(), INTERVAL days_var DAY), 
                type_var, 
                TRUE
            );
        END IF;

        SET i = i + 1;
    END WHILE;
END //

DELIMITER ;

-- Chạy Procedure
CALL SeedUsers();

-- Xóa Procedure sau khi dùng xong
DROP PROCEDURE SeedUsers;

SELECT '✅ Đã thêm 50 users và licenses thành công!' AS Result;
