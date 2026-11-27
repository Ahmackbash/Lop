using System.Text.Json;

namespace Library.App.Services;

/// <summary>Результат загрузки: данные и предупреждение (например, о восстановлении из копии).</summary>
public sealed record StoreLoadResult<T>(IReadOnlyList<T> Items, string? Warning);

public interface IFileStore<T>
{
    Task<StoreLoadResult<T>> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(IReadOnlyList<T> items, CancellationToken cancellationToken);
}

/// <summary>
/// Хранилище списка сущностей в JSON: копия перед каждой записью,
/// восстановление последней успешной копии при JsonException.
/// </summary>
public sealed class JsonFileStore<T> : IFileStore<T>
{
    private readonly string _filePath;
    private readonly BackupService _backups;
    private readonly JsonSerializerOptions _options;

    public JsonFileStore(string filePath, BackupService backups, JsonSerializerOptions options)
    {
        _filePath = filePath;
        _backups = backups;
        _options = options;
    }

    public async Task<StoreLoadResult<T>> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath)) return new StoreLoadResult<T>(Array.Empty<T>(), null);

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        try
        {
            return new StoreLoadResult<T>(Deserialize(json), null);
        }
        catch (JsonException)
        {
            // Файл повреждён: сохраняем его для разбора и пробуем копии от новой к старой.
            await _backups.PreserveCorruptedFileAsync(_filePath, cancellationToken);

            foreach (var backupPath in _backups.GetBackupsNewestFirst(_filePath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var items = Deserialize(await File.ReadAllTextAsync(backupPath, cancellationToken));
                    await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(items, _options), cancellationToken);
                    return new StoreLoadResult<T>(items,
                        $"Файл «{Path.GetFileName(_filePath)}» повреждён. Данные восстановлены из копии «{Path.GetFileName(backupPath)}».");
                }
                catch (JsonException)
                {
                    // Копия тоже повреждена — пробуем следующую.
                }
            }

            return new StoreLoadResult<T>(Array.Empty<T>(),
                $"Файл «{Path.GetFileName(_filePath)}» повреждён, пригодных резервных копий нет. Список начат пустым.");
        }
    }

    public async Task SaveAsync(IReadOnlyList<T> items, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        await _backups.CreateBackupAsync(_filePath, cancellationToken);

        // Запись во временный файл и замена одним действием: основной файл не остаётся «половиной».
        var tempPath = _filePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(items, _options), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        File.Move(tempPath, _filePath, overwrite: true);
    }

    private IReadOnlyList<T> Deserialize(string json)
        => JsonSerializer.Deserialize<List<T>>(json, _options)
           ?? throw new JsonException("Документ JSON не содержит массива данных.");
}