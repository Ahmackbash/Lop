using System.Collections.ObjectModel;
using System.IO;
using Library.App.Infrastructure;
using Library.App.Models;

namespace Library.App.Services;

/// <summary>
/// Рабочее пространство данных: коллекции приложения, асинхронная загрузка/сохранение,
/// состояние занятости, отмена и обработка файловых ошибок в одном месте.
/// </summary>
public sealed class LibraryWorkspace : ObservableModel
{
    private readonly ILibraryRepository _repository;
    private readonly ICatalogImporter _importer;
    private readonly AppPaths _paths;
    private readonly ApplicationState _state;
    private readonly IDialogService _dialogs;
    private bool _hasPendingChanges;

    public LibraryWorkspace(ILibraryRepository repository, ICatalogImporter importer, AppPaths paths,
        ApplicationState state, IDialogService dialogs)
    {
        _repository = repository;
        _importer = importer;
        _paths = paths;
        _state = state;
        _dialogs = dialogs;

        Loans.CollectionChanged += (_, _) => LoansChanged?.Invoke(this, EventArgs.Empty);
    }

    public ObservableCollection<Book> Books { get; } = new();
    public ObservableCollection<Reader> Readers { get; } = new();
    public ObservableCollection<Loan> Loans { get; } = new();

    /// <summary>Выдачи изменились (в том числе возврат) — экраны обновляют доступность и строки.</summary>
    public event EventHandler? LoansChanged;

    public bool HasPendingChanges
    {
        get => _hasPendingChanges;
        private set => SetProperty(ref _hasPendingChanges, value);
    }

    public void NotifyLoansChanged() => LoansChanged?.Invoke(this, EventArgs.Empty);

    public async Task LoadAsync()
    {
        if (_state.IsBusy) return;

        var token = _state.BeginOperation("Загрузка данных...");
        var status = "Готово";
        try
        {
            var snapshot = await _repository.LoadAsync(token);

            CollectionHelper.Replace(Books, snapshot.Books);
            CollectionHelper.Replace(Readers, snapshot.Readers);
            CollectionHelper.Replace(Loans, snapshot.Loans);
            HasPendingChanges = false;

            if (snapshot.Warnings.Count > 0)
                _dialogs.ShowWarning(string.Join(Environment.NewLine, snapshot.Warnings), "Восстановление данных");

            if (Books.Count == 0) await SeedCatalogAsync(token);

            status = $"Загружено: книг — {Books.Count}, читателей — {Readers.Count}, выдач — {Loans.Count}";
        }
        catch (OperationCanceledException)
        {
            status = "Загрузка отменена";
        }
        catch (Exception ex)
        {
            status = "Ошибка загрузки";
            _dialogs.ShowError("Не удалось загрузить данные", ex);
        }
        finally
        {
            _state.EndOperation(status);
        }
    }

    public async Task SaveAsync(string statusMessage = "Сохранение данных...")
    {
        if (_state.IsBusy) return;

        var token = _state.BeginOperation(statusMessage);
        var status = "Готово";
        try
        {
            await Task.Delay(3000, token); // ВРЕМЕННО: демонстрация IsBusy и отмены на защите
            await SaveWithinOperationAsync(token);
            status = $"Сохранено в {DateTime.Now:HH:mm:ss}";
        }
        catch (OperationCanceledException)
        {
            status = "Сохранение отменено";
        }
        catch (Exception ex)
        {
            status = "Ошибка сохранения";
            _dialogs.ShowError("Не удалось сохранить данные", ex);
        }
        finally
        {
            _state.EndOperation(status);
        }
    }

    /// <summary>Изменение данных и немедленная запись (перед каждой записью создаётся резервная копия).</summary>
    public async Task CommitAsync(string statusMessage)
    {
        HasPendingChanges = true;
        await SaveAsync(statusMessage);
    }

    /// <summary>Запись внутри уже начатой операции (импорт, первичное заполнение).</summary>
    public async Task SaveWithinOperationAsync(CancellationToken cancellationToken)
    {
        await _repository.SaveAsync(Books.ToList(), Readers.ToList(), Loans.ToList(), cancellationToken);
        HasPendingChanges = false;
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_paths.SeedCatalogFile)) return;

        _state.StatusMessage = "Каталог пуст: первичный импорт из books.csv...";
        var result = await _importer.ImportAsync(_paths.SeedCatalogFile, cancellationToken);

        foreach (var book in result.Books) Books.Add(book);
        if (result.Books.Count == 0) return;

        await SaveWithinOperationAsync(cancellationToken);
        _dialogs.ShowInformation(
            $"Каталог был пуст — импортировано книг: {result.Books.Count} из data/books.csv.",
            "Первичный импорт");
    }
}