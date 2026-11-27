using System.Windows.Input;

namespace Library.App.Infrastructure;

/// <summary>
/// Команда для асинхронных операций: на время выполнения сама себя блокирует,
/// исключения передаёт обработчику (приложение не падает).
/// </summary>
public sealed class AsyncRelayCommand : ICommand, IRefreshableCommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _onError;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onError = onError;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    // ICommand требует void; вся асинхронная работа и обработка ошибок внутри.
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();
        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            _onError?.Invoke(ex);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}