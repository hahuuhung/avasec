using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;
using AVASec.Database;

namespace AVASec.Authentication.Services
{
    /// <summary>
    /// Authentication service implementation / Triển khai dịch vụ xác thực
    /// Handles user registration, login, and password management / Xử lý đăng ký, đăng nhập và quản lý mật khẩu
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AVASecContext _context;

        public AuthenticationService(AVASecContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Register new user / Đăng ký người dùng mới
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email)
        {
            try
            {
                // Validate input / Kiểm tra đầu vào
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                {
                    return (false, "All fields are required. / Tất cả các trường là bắt buộc.");
                }

                // Check if username exists / Kiểm tra tên đăng nhập đã tồn tại
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                if (existingUser != null)
                {
                    return (false, "Username already exists. / Tên đăng nhập đã tồn tại.");
                }

                // Check if email exists / Kiểm tra email đã tồn tại
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
                
                if (existingEmail != null)
                {
                    return (false, "Email already registered. / Email đã được đăng ký.");
                }

                // Hash password / Mã hóa mật khẩu
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Create user / Tạo người dùng
                var newUser = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Email = email,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Create trial license (14 days) / Tạo giấy phép dùng thử (14 ngày)
                var trialLicense = new License
                {
                    UserId = newUser.UserId,
                    LicenseKey = GenerateLicenseKey(),
                    IssueDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(14),
                    IsActive = true,
                    LicenseType = "Trial"
                };

                _context.Licenses.Add(trialLicense);
                await _context.SaveChangesAsync();

                return (true, $"Registration successful! Trial license valid until {trialLicense.ExpiryDate:yyyy-MM-dd}. / Đăng ký thành công! Giấy phép dùng thử có hiệu lực đến {trialLicense.ExpiryDate:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message} / Đăng ký thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Login user / Đăng nhập người dùng
        /// </summary>
        public async Task<(bool Success, int? UserId, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                // Find user / Tìm người dùng
                var user = await _context.Users
                    .Include(u => u.License)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return (false, null, "Invalid username or password. / Tên đăng nhập hoặc mật khẩu không đúng.");
                }

                // Verify password / Xác minh mật khẩu
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return (false, null, "Invalid username or password. / Tên đăng nhập hoặc mật khẩu không đúng.");
                }

                // Check if account is active / Kiểm tra tài khoản có hoạt động
                if (!user.IsActive)
                {
                    return (false, null, "Account is deactivated. / Tài khoản đã bị vô hiệu hóa.");
                }

                // Check license validity / Kiểm tra giấy phép còn hiệu lực
                if (user.License == null || !user.License.IsValid())
                {
                    return (false, null, "License has expired. Please renew. / Giấy phép đã hết hạn. Vui lòng gia hạn.");
                }

                // Get remaining days / Lấy số ngày còn lại
                int remainingDays = user.License.GetRemainingDays();
                
                return (true, user.UserId, $"Login successful! License expires in {remainingDays} days. / Đăng nhập thành công! Giấy phép hết hạn sau {remainingDays} ngày.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Login failed: {ex.Message} / Đăng nhập thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user by ID / Lấy thông tin người dùng theo ID
        /// </summary>
        public async Task<AVASec.Core.Models.User?> GetUserAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.License)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Change user password / Đổi mật khẩu người dùng
        /// </summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return (false, "User not found. / Không tìm thấy người dùng.");
                }

                // Verify old password / Xác minh mật khẩu cũ
                bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);

                if (!isOldPasswordValid)
                {
                    return (false, "Old password is incorrect. / Mật khẩu cũ không đúng.");
                }

                // Hash new password / Mã hóa mật khẩu mới
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                await _context.SaveChangesAsync();

                return (true, "Password changed successfully. / Đổi mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Password change failed: {ex.Message} / Đổi mật khẩu thất bại: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate unique license key / Tạo mã giấy phép duy nhất
        /// </summary>
        private string GenerateLicenseKey()
        {
            return $"SA-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}
