using System.Windows.Input;

namespace Library.App.Infrastructure;

/// <summary>Команда, у которой можно принудительно пересчитать CanExecute.</summary>
public interface IRefreshableCommand
{
    void RaiseCanExecuteChanged();
}

/// <summary>Ручная реализация ICommand для синхронных действий.</summary>
public sealed class RelayCommand : ICommand, IRefreshableCommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter)) _execute();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}