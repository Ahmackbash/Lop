using Library.App.Models;

namespace Library.App.Services;

/// <summary>Бизнес-правила выдач, возвратов, просрочек и штрафов.</summary>
public sealed class LoanService
{
    /// <summary>Первые 7 дней просрочки бесплатны.</summary>
    public const int FreeOverdueDays = 7;

    /// <summary>Далее 5 единиц за каждый день просрочки.</summary>
    public const decimal FinePerDay = 5m;

    public const int DefaultLoanTermDays = 14;

    public int GetActiveLoanCount(string isbn, IEnumerable<Loan> loans)
        => loans.Count(loan => loan.BookIsbn == isbn && !loan.IsReturned);

    public int GetAvailableCopies(Book book, IEnumerable<Loan> loans)
        => Math.Max(0, book.TotalCopies - GetActiveLoanCount(book.Isbn, loans));

    public bool CanIssue(Book book, IEnumerable<Loan> loans) => GetAvailableCopies(book, loans) > 0;

    /// <summary>Дни просрочки: для возвращённых — до фактической даты, иначе до указанной (по умолчанию сегодня).</summary>
    public int GetOverdueDays(Loan loan, DateTime? referenceDate = null)
    {
        var end = (loan.ReturnDate ?? referenceDate ?? DateTime.Today).Date;
        return Math.Max(0, (end - loan.DueDate.Date).Days);
    }

    /// <summary>Штраф = max(0, дни просрочки − 7) × 5.</summary>
    public decimal GetFine(Loan loan, DateTime? referenceDate = null)
        => Math.Max(0, GetOverdueDays(loan, referenceDate) - FreeOverdueDays) * FinePerDay;

    /// <summary>Срок чтения в днях (для активных выдач — до указанной даты).</summary>
    public double GetReadingDays(Loan loan, DateTime? referenceDate = null)
        => ((loan.ReturnDate ?? referenceDate ?? DateTime.Today).Date - loan.IssueDate.Date).TotalDays;

    public Loan CreateLoan(Book book, Reader reader, DateTime issueDate, int termDays, IEnumerable<Loan> loans)
    {
        if (!CanIssue(book, loans))
            throw new DomainException($"Свободных экземпляров книги «{book.Title}» нет.");
        if (termDays <= 0)
            throw new DomainException("Срок выдачи должен быть положительным числом дней.");

        return new Loan
        {
            Id = Guid.NewGuid(),
            BookIsbn = book.Isbn,
            ReaderTicketNumber = reader.TicketNumber,
            IssueDate = issueDate.Date,
            DueDate = issueDate.Date.AddDays(termDays),
            ReturnDate = null
        };
    }

    public void Return(Loan loan, DateTime returnDate)
    {
        if (loan.IsReturned) throw new DomainException("Книга уже возвращена.");
        if (returnDate.Date < loan.IssueDate.Date)
            throw new DomainException("Дата возврата не может быть раньше даты выдачи.");

        loan.ReturnDate = returnDate.Date;
    }
}