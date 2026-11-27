namespace Library.App.Models;

/// <summary>Строка отчёта «топ книг по числу выдач».</summary>
public sealed record TopBookRow(string Isbn, string Title, string Author, string Genre, int LoanCount);

/// <summary>Строка отчёта «читатели с просрочками».</summary>
public sealed record OverdueReaderRow(
    string TicketNumber, string FullName, string Phone,
    int OverdueLoanCount, int TotalOverdueDays, int MaxOverdueDays, decimal TotalFine);

/// <summary>Строка отчёта «распределение выдач по жанрам».</summary>
public sealed record GenreShareRow(string Genre, int LoanCount, double SharePercent);

/// <summary>Сводные показатели, включая средний срок чтения.</summary>
public sealed record LibrarySummary(
    int BookCount, int CopyCount, int ReaderCount, int LoanCount, int ActiveLoanCount,
    int OverdueLoanCount, decimal TotalFine, double AverageReadingDays, int ReturnedLoanCount);

/// <summary>
/// Итоговый отчёт. Секции nullable: при выгрузке отключённые секции не попадают в JSON.
/// </summary>
public sealed record LibraryReport
{
    public DateTime GeneratedAt { get; init; }
    public DateTime? PeriodFrom { get; init; }
    public DateTime? PeriodTo { get; init; }
    public LibrarySummary? Summary { get; init; }
    public IReadOnlyList<TopBookRow>? TopBooks { get; init; }
    public IReadOnlyList<OverdueReaderRow>? OverdueReaders { get; init; }
    public IReadOnlyList<GenreShareRow>? GenreDistribution { get; init; }
}