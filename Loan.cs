using System.Text.Json.Serialization;

namespace Library.App.Models;

/// <summary>Выдача экземпляра книги читателю.</summary>
public sealed class Loan : ObservableModel
{
    private Guid _id = Guid.NewGuid();
    private string _bookIsbn = string.Empty;
    private string _readerTicketNumber = string.Empty;
    private DateTime _issueDate = DateTime.Today;
    private DateTime _dueDate = DateTime.Today.AddDays(14);
    private DateTime? _returnDate;

    public Guid Id { get => _id; set => SetProperty(ref _id, value); }
    public string BookIsbn { get => _bookIsbn; set => SetProperty(ref _bookIsbn, value); }
    public string ReaderTicketNumber { get => _readerTicketNumber; set => SetProperty(ref _readerTicketNumber, value); }
    public DateTime IssueDate { get => _issueDate; set => SetProperty(ref _issueDate, value); }

    /// <summary>Срок возврата по договору.</summary>
    public DateTime DueDate { get => _dueDate; set => SetProperty(ref _dueDate, value); }

    /// <summary>Фактическая дата возврата; null — книга ещё на руках.</summary>
    public DateTime? ReturnDate
    {
        get => _returnDate;
        set
        {
            if (SetProperty(ref _returnDate, value)) OnPropertyChanged(nameof(IsReturned));
        }
    }

    [JsonIgnore]
    public bool IsReturned => ReturnDate.HasValue;
}