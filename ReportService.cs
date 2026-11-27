using Library.App.Models;

namespace Library.App.Services;

/// <summary>LINQ-отчёты по каталогу, читателям и выдачам.</summary>
public sealed class ReportService
{
    private readonly LoanService _loanService;

    public ReportService(LoanService loanService) => _loanService = loanService;

    public LibraryReport Build(IReadOnlyList<Book> books, IReadOnlyList<Reader> readers, IReadOnlyList<Loan> loans,
        DateTime? periodFrom, DateTime? periodTo)
    {
        var today = DateTime.Today;

        var filtered = loans
            .Where(loan => (periodFrom is null || loan.IssueDate.Date >= periodFrom.Value.Date)
                        && (periodTo is null || loan.IssueDate.Date <= periodTo.Value.Date))
            .ToList();

        // Топ-10 книг по числу выдач.
        var topBooks = filtered
            .GroupBy(loan => loan.BookIsbn)
            .Join(books, group => group.Key, book => book.Isbn,
                (group, book) => new TopBookRow(book.Isbn, book.Title, book.Author, book.Genre, group.Count()))
            .OrderByDescending(row => row.LoanCount)
            .ThenBy(row => row.Title, StringComparer.CurrentCulture)
            .Take(10)
            .ToList();

        // Читатели с просрочками: количество, суммарные дни, максимум и штраф.
        var overdueReaders = filtered
            .Select(loan => (Loan: loan, Days: _loanService.GetOverdueDays(loan, today), Fine: _loanService.GetFine(loan, today)))
            .Where(item => item.Days > 0)
            .GroupBy(item => item.Loan.ReaderTicketNumber)
            .Join(readers, group => group.Key, reader => reader.TicketNumber,
                (group, reader) => new OverdueReaderRow(
                    reader.TicketNumber, reader.FullName, reader.Phone,
                    group.Count(), group.Sum(item => item.Days), group.Max(item => item.Days),
                    group.Sum(item => item.Fine)))
            .OrderByDescending(row => row.TotalFine)
            .ThenByDescending(row => row.TotalOverdueDays)
            .ToList();

        // Распределение выдач по жанрам.
        var genresOfLoans = filtered
            .Join(books, loan => loan.BookIsbn, book => book.Isbn, (_, book) => book.Genre)
            .ToList();

        var genreDistribution = genresOfLoans
            .GroupBy(genre => genre)
            .Select(group => new GenreShareRow(
                group.Key, group.Count(),
                genresOfLoans.Count == 0 ? 0 : Math.Round(100.0 * group.Count() / genresOfLoans.Count, 2)))
            .OrderByDescending(row => row.LoanCount)
            .ThenBy(row => row.Genre, StringComparer.CurrentCulture)
            .ToList();

        // Средний срок чтения — по фактически возвращённым книгам.
        var returned = filtered.Where(loan => loan.IsReturned).ToList();
        var averageReadingDays = returned.Count == 0
            ? 0
            : Math.Round(returned.Average(loan => _loanService.GetReadingDays(loan, today)), 2);

        var summary = new LibrarySummary(
            books.Count,
            books.Sum(book => book.TotalCopies),
            readers.Count,
            filtered.Count,
            filtered.Count(loan => !loan.IsReturned),
            filtered.Count(loan => _loanService.GetOverdueDays(loan, today) > 0),
            filtered.Sum(loan => _loanService.GetFine(loan, today)),
            averageReadingDays,
            returned.Count);

        return new LibraryReport
        {
            GeneratedAt = DateTime.Now,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            Summary = summary,
            TopBooks = topBooks,
            OverdueReaders = overdueReaders,
            GenreDistribution = genreDistribution
        };
    }
}