using System.Globalization;

namespace Library.App.Services;

/// <summary>Резервные копии файлов данных с меткой времени.</summary>
public sealed class BackupService
{
    private const int MaxBackupsPerFile = 20;
    private readonly AppPaths _paths;

    public BackupService(AppPaths paths) => _paths = paths;

    /// <summary>Создаёт копию файла перед записью. Возвращает путь копии или null, если файла ещё нет.</summary>
    public async Task<string?> CreateBackupAsync(string dataFilePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(dataFilePath)) return null;

        _paths.EnsureDirectories();
        var backupPath = BuildBackupPath(dataFilePath);
        await CopyFileAsync(dataFilePath, backupPath, cancellationToken);
        TrimOldBackups(dataFilePath);
        return backupPath;
    }

    /// <summary>Сохраняет повреждённый файл отдельно, чтобы он не считался рабочей копией.</summary>
    public Task PreserveCorruptedFileAsync(string dataFilePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(dataFilePath)) return Task.CompletedTask;

        _paths.EnsureDirectories();
        return CopyFileAsync(dataFilePath, BuildBackupPath(dataFilePath, "corrupted"), cancellationToken);
    }

    /// <summary>Рабочие копии от самой новой к самой старой (метка времени в имени задаёт порядок).</summary>
    public IReadOnlyList<string> GetBackupsNewestFirst(string dataFilePath)
    {
        if (!Directory.Exists(_paths.BackupDirectory)) return Array.Empty<string>();

        var name = Path.GetFileNameWithoutExtension(dataFilePath);
        return Directory.EnumerateFiles(_paths.BackupDirectory, $"{name}_*.json")
            .Where(path => !Path.GetFileName(path).Contains("_corrupted_", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToList();
    }

    private string BuildBackupPath(string dataFilePath, string? marker = null)
    {
        var name = Path.GetFileNameWithoutExtension(dataFilePath);
        var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff", CultureInfo.InvariantCulture);
        var suffix = marker is null ? string.Empty : $"_{marker}";
        return Path.Combine(_paths.BackupDirectory, $"{name}{suffix}_{stamp}.json");
    }

    private static async Task CopyFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken)
    {
        await using var source = File.OpenRead(sourcePath);
        await using var target = File.Create(targetPath);
        await source.CopyToAsync(target, cancellationToken);
    }

    private void TrimOldBackups(string dataFilePath)
    {
        foreach (var path in GetBackupsNewestFirst(dataFilePath).Skip(MaxBackupsPerFile))
        {
            try { File.Delete(path); }
            catch (IOException) { /* удаление старых копий не критично */ }
            catch (UnauthorizedAccessException) { /* нет прав — пропускаем */ }
        }
    }
}