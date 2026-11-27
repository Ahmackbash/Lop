using System.IO;
using System.Windows;
using Library.App.Models;
using Library.App.Services;
using Library.App.ViewModels;
using Microsoft.Win32;

namespace Library.App.Views;

/// <summary>Реализация диалогов в слое Views: ViewModel знает только об интерфейсе.</summary>
public sealed class DialogService : IDialogService
{
    public void ShowInformation(string message, string title = "Библиотека")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "Внимание")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, Exception? exception = null)
    {
        var text = exception is null ? message : $"{message}{Environment.NewLine}{exception.Message}";
        MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string message, string title = "Подтверждение")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public bool? ConfirmSaveChanges()
    {
        var answer = MessageBox.Show("Есть несохранённые изменения. Сохранить их перед выходом?",
            "Выход", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        return answer switch
        {
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => false,
            _ => null
        };
    }

    public string? ChooseCsvFile(string? initialFilePath)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Импорт каталога из CSV",
            Filter = "CSV-файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
            CheckFileExists = true
        };

        if (!string.IsNullOrEmpty(initialFilePath) && File.Exists(initialFilePath)) dialog.FileName = initialFilePath;
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ChooseJsonReportFile(string suggestedFileName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Выгрузка отчёта в JSON",
            Filter = "JSON-файлы (*.json)|*.json",
            FileName = suggestedFileName,
            DefaultExt = ".json",
            OverwritePrompt = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public Book? EditBook(Book? source, IReadOnlyList<Book> catalog, int issuedCopies, bool isIsbnLocked)
    {
        var editor = new BookEditorViewModel(source, catalog, issuedCopies, isIsbnLocked);
        var window = new BookEditorWindow { DataContext = editor, Owner = Application.Current.MainWindow };
        editor.CloseRequested += (_, accepted) => window.DialogResult = accepted;

        return window.ShowDialog() == true ? editor.Result : null;
    }

    public Reader? EditReader(Reader? source, IReadOnlyList<Reader> readers, bool isTicketLocked)
    {
        var editor = new ReaderEditorViewModel(source, readers, isTicketLocked);
        var window = new ReaderEditorWindow { DataContext = editor, Owner = Application.Current.MainWindow };
        editor.CloseRequested += (_, accepted) => window.DialogResult = accepted;

        return window.ShowDialog() == true ? editor.Result : null;
    }
}