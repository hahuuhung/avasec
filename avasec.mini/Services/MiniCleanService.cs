using System.IO;

namespace AVASec.Mini.Services;

public sealed class MiniCleanService
{
    public async Task<long> CleanJunkAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            long total = 0;
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Add(string? p) { if (!string.IsNullOrWhiteSpace(p)) roots.Add(p); }

            Add(Path.GetTempPath());
            Add(Environment.GetEnvironmentVariable("TEMP"));
            Add(Environment.GetEnvironmentVariable("TMP"));
            Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"));
            Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));

            foreach (var root in roots)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(root)) continue;
                progress?.Report("Dang don / Cleaning: " + root);
                total += DeleteSafeContents(root, cancellationToken);
            }
            return total;
        }, cancellationToken);
    }

    private static long DeleteSafeContents(string directory, CancellationToken token)
    {
        long freed = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var info = new FileInfo(file);
                    long len = info.Length;
                    info.IsReadOnly = false;
                    info.Delete();
                    freed += len;
                }
                catch { }
            }
        }
        catch { }
        return freed;
    }

    public static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double value = bytes;
        int i = 0;
        while (value >= 1024 && i < units.Length - 1) { value /= 1024; i++; }
        return $"{value:0.#} {units[i]}";
    }
}