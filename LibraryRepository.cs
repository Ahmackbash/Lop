using Library.App.Models;

namespace Library.App.Services;

public sealed record LibrarySnapshot(
    IReadOnlyList<Book> Books, IReadOnlyList<Reader> Readers, IReadOnlyList<Loan> Loans,
    IReadOnlyList<string> Warnings);

public interface ILibraryRepository
{
    Task<LibrarySnapshot> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(IReadOnlyList<Book> books, IReadOnlyList<Reader> readers, IReadOnlyList<Loan> loans,
        CancellationToken cancellationToken);
}

/// <summary>Согласует три файла данных: books.json, readers.json, loans.json.</summary>
public sealed class LibraryRepository : ILibraryRepository
{
    private readonly IFileStore<Book> _books;
    private readonly IFileStore<Reader> _readers;
    private readonly IFileStore<Loan> _loans;
    private readonly AppPaths _paths;

    public LibraryRepository(IFileStore<Book> books, IFileStore<Reader> readers, IFileStore<Loan> loans, AppPaths paths)
    {
        _books = books;
        _readers = readers;
        _loans = loans;
        _paths = paths;
    }

    public async Task<LibrarySnapshot> LoadAsync(CancellationToken cancellationToken)
    {
        _paths.EnsureDirectories();

        var books = await _books.LoadAsync(cancellationToken);
        var readers = await _readers.LoadAsync(cancellationToken);
        var loans = await _loans.LoadAsync(cancellationToken);

        var warnings = new[] { books.Warning, readers.Warning, loans.Warning }
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Select(warning => warning!)
            .ToList();

        return new LibrarySnapshot(books.Items, readers.Items, loans.Items, warnings);
    }

    public async Task SaveAsync(IReadOnlyList<Book> books, IReadOnlyList<Reader> readers, IReadOnlyList<Loan> loans,
        CancellationToken cancellationToken)
    {
        _paths.EnsureDirectories();

        // Последовательно: предсказуемый порядок резервных копий и корректная отмена.
        await _books.SaveAsync(books, cancellationToken);
        await _readers.SaveAsync(readers, cancellationToken);
        await _loans.SaveAsync(loans, cancellationToken);
    }
}