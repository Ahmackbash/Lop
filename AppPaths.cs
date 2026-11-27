namespace Library.App.Services;

/// <summary>Все пути строятся от AppContext.BaseDirectory через Path.Combine.</summary>
public sealed class AppPaths
{
    public AppPaths()
    {
        DataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        BackupDirectory = Path.Combine(DataDirectory, "backup");
        BooksFile = Path.Combine(DataDirectory, "books.json");
        ReadersFile = Path.Combine(DataDirectory, "readers.json");
        LoansFile = Path.Combine(DataDirectory, "loans.json");
        SeedCatalogFile = Path.Combine(DataDirectory, "books.csv");
    }

    public string DataDirectory { get; }
    public string BackupDirectory { get; }
    public string BooksFile { get; }
    public string ReadersFile { get; }
    public string LoansFile { get; }

    /// <summary>Исходный каталог для импорта (лежит в репозитории и копируется в вывод).</summary>
    public string SeedCatalogFile { get; }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(BackupDirectory);
    }
}