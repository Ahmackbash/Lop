using Library.App.Models;

namespace Library.App.Infrastructure;

/// <summary>
/// Базовый класс ViewModel: наследует уведомление об изменении свойств,
/// ведёт реестр команд и пробрасывает состояние занятости приложения.
/// </summary>
public abstract class ViewModelBase : ObservableModel
{
    private readonly List<IRefreshableCommand> _commands = new();
    private ApplicationState? _state;

    public bool IsBusy => _state?.IsBusy ?? false;

    /// <summary>Подписка на общее состояние: блокировка команд на время операций.</summary>
    protected void BindBusyState(ApplicationState state)
    {
        _state = state;
        _state.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsBusy));
            RefreshCommands();
        };
    }

    protected RelayCommand RegisterCommand(Action execute, Func<bool>? canExecute = null)
    {
        var command = new RelayCommand(execute, canExecute);
        _commands.Add(command);
        return command;
    }

    protected AsyncRelayCommand RegisterAsyncCommand(
        Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
    {
        var command = new AsyncRelayCommand(execute, canExecute, onError);
        _commands.Add(command);
        return command;
    }

    /// <summary>Пересчёт CanExecute всех команд экрана.</summary>
    protected void RefreshCommands()
    {
        foreach (var command in _commands) command.RaiseCanExecuteChanged();
    }
}