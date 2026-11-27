using Library.App.Models;

namespace Library.App.Services;

/// <summary>
/// Абстракция диалогов: ViewModel не знает об окнах и элементах управления.
/// Реализация лежит в слое Views.
/// </summary>
public interface IDialogService
{
    void ShowInformation(string message, string title = "Библиотека");
    void ShowWarning(string message, string title = "Внимание");
    void ShowError(string message, Exception? exception = null);
    bool Confirm(string message, string title = "Подтверждение");

    /// <summary>true — сохранить, false — не сохранять, null — отменить закрытие.</summary>
    bool? ConfirmSaveChanges();

    string? ChooseCsvFile(string? initialFilePath);
    string? ChooseJsonReportFile(string suggestedFileName);

    Book? EditBook(Book? source, IReadOnlyList<Book> catalog, int issuedCopies, bool isIsbnLocked);
    Reader? EditReader(Reader? source, IReadOnlyList<Reader> readers, bool isTicketLocked);
}