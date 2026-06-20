using Microsoft.EntityFrameworkCore;
using AVASec.Authentication.Services;
using AVASec.Database;
using AVASec.Core.Models;
using Xunit;

namespace AVASec.Tests
{
    public class AuthenticationServiceTests
    {
        private DbContextOptions<AVASecContext> _dbOptions;

        public AuthenticationServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AVASecContext>()
                .UseInMemoryDatabase(databaseName: "AVASecTestDb")
                .Options;
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            using var context = new AVASecContext(_dbOptions);
            var service = new AuthenticationService(context);

            string username = "testuser";
            string password = "password123";
            string email = "test@example.com";

            // Act
            var result = await service.RegisterAsync(username, password, email);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(context.Users.FirstOrDefault(u => u.Username == username));
        }

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenUsernameExists()
        {
            // Arrange
            using var context = new AVASecContext(_dbOptions);
            context.Users.Add(new User { Username = "existinguser", PasswordHash = "hash", Email = "old@example.com" });
            context.SaveChanges();
            
            var service = new AuthenticationService(context);

            // Act
            var result = await service.RegisterAsync("existinguser", "newpass", "new@example.com");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already exists. / Tên đăng nhập đã tồn tại.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreCorrect()
        {
            // Arrange
            using var context = new AVASecContext(_dbOptions);
            string uniqueUser = "loginuser" + Guid.NewGuid();
            string correctPassword = "correctpassword";
            // Manually hash using BCrypt because the service verifies using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            context.Users.Add(new User 
            { 
                Username = uniqueUser, 
                PasswordHash = hashedPassword, 
                Email = "login@example.com",
                License = new License {  LicenseKey = "TEST", ExpiryDate = DateTime.Now.AddDays(1) } // Active License
            });
            context.SaveChanges();

            var service = new AuthenticationService(context);

            // Act
            var result = await service.LoginAsync(uniqueUser, correctPassword);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.NotNull(result.UserId);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsIncorrect()
        {
             // Arrange
            using var context = new AVASecContext(_dbOptions);
             string uniqueUser = "failuser" + Guid.NewGuid();
             string correctPassword = "correctpassword";
             string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            context.Users.Add(new User 
            { 
                Username = uniqueUser, 
                PasswordHash = hashedPassword, 
                Email = "fail@example.com" 
            });
            context.SaveChanges();

            var service = new AuthenticationService(context);

            // Act
            var result = await service.LoginAsync(uniqueUser, "wrongpassword");

            // Assert
            Assert.False(result.Success);
        }
    }
}
