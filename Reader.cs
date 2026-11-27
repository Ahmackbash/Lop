namespace Library.App.Models;

/// <summary>Читатель библиотеки. Номер билета — уникальный ключ.</summary>
public sealed class Reader : ObservableModel
{
    private string _ticketNumber = string.Empty;
    private string _fullName = string.Empty;
    private string _phone = string.Empty;
    private DateTime _registrationDate = DateTime.Today;

    public string TicketNumber { get => _ticketNumber; set => SetProperty(ref _ticketNumber, value); }
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    public DateTime RegistrationDate { get => _registrationDate; set => SetProperty(ref _registrationDate, value); }

    public void CopyFrom(Reader other)
    {
        TicketNumber = other.TicketNumber;
        FullName = other.FullName;
        Phone = other.Phone;
        RegistrationDate = other.RegistrationDate;
    }
}