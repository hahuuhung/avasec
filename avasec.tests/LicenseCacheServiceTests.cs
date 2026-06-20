using AVASec.Core.Models;
using AVASec.Core.Services;

namespace AVASec.Tests;

public class LicenseCacheServiceTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsSnapshot()
    {
        var service = new LicenseCacheService();
        var snapshot = new CachedLicenseSnapshot
        {
            UserId = 42,
            Username = "testuser",
            LicenseKey = "AVA-TEST-KEY",
            LicenseType = "Trial",
            ExpiryDate = DateTime.UtcNow.AddDays(14),
            IsActive = true
        };

        service.Save(snapshot);
        var loaded = service.Load();

        Assert.NotNull(loaded);
        Assert.Equal("testuser", loaded!.Username);
        Assert.Equal("AVA-TEST-KEY", loaded.LicenseKey);
    }

    [Fact]
    public void IsWithinGracePeriod_ReturnsTrueForRecentCache()
    {
        var service = new LicenseCacheService();
        service.Save(new CachedLicenseSnapshot
        {
            LicenseType = "Ultra",
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        });

        Assert.True(service.IsWithinGracePeriod());
    }
}
