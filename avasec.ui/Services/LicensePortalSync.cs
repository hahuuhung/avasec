using Microsoft.EntityFrameworkCore;
using AVASec.Core.Models;
using AVASec.Core.Services;
using AVASec.Database;

namespace AVASec.UI.Services;

public static class LicensePortalSync
{
    public static async Task SyncUserLicenseToLocalAsync(User? webUser)
    {
        if (webUser?.License == null)
        {
            return;
        }

        await using var context = new AVASecContext();
        var localUser = await context.Users
            .Include(u => u.License)
            .FirstOrDefaultAsync(u => u.Username == webUser.Username);

        if (localUser == null)
        {
            localUser = new User
            {
                Username = webUser.Username,
                Email = webUser.Email,
                PasswordHash = string.IsNullOrEmpty(webUser.PasswordHash) ? "portal-sync" : webUser.PasswordHash,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(localUser);
            await context.SaveChangesAsync();
        }

        var lic = webUser.License;
        if (localUser.License == null)
        {
            context.Licenses.Add(new License
            {
                UserId = localUser.UserId,
                LicenseKey = lic.LicenseKey,
                LicenseType = lic.LicenseType,
                IssueDate = lic.IssueDate,
                ExpiryDate = lic.ExpiryDate,
                IsActive = lic.IsActive
            });
        }
        else
        {
            localUser.License.LicenseKey = lic.LicenseKey;
            localUser.License.LicenseType = lic.LicenseType;
            localUser.License.ExpiryDate = lic.ExpiryDate;
            localUser.License.IsActive = lic.IsActive;
        }

        await context.SaveChangesAsync();

        new LicenseCacheService().Save(new CachedLicenseSnapshot
        {
            UserId = webUser.UserId,
            Username = webUser.Username,
            LicenseKey = lic.LicenseKey,
            LicenseType = lic.LicenseType,
            ExpiryDate = lic.ExpiryDate,
            IsActive = lic.IsActive
        });
    }

    public static void ApplyToFeatureGate(FeatureGateService gate, License license)
    {
        gate.SetLicense(license.LicenseType, license.IsValid(), license.GetRemainingDays());
    }
}
