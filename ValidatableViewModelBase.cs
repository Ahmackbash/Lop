using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Library.App.Infrastructure;

/// <summary>
/// База для ViewModel с валидацией ввода через INotifyDataErrorInfo.
/// Правила задаются делегатами, ошибки показываются в привязках WPF.
/// </summary>
public abstract class ValidatableViewModelBase : ViewModelBase, INotifyDataErrorInfo
{
    private readonly Dictionary<string, Func<string?>> _validators = new();
    private readonly Dictionary<string, string> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
        => propertyName is not null && _errors.TryGetValue(propertyName, out var error)
            ? new[] { error }
            : Array.Empty<string>();

    protected void AddValidator(string propertyName, Func<string?> validator)
        => _validators[propertyName] = validator;

    protected override bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        var changed = base.SetProperty(ref field, value, propertyName);
        if (changed && propertyName is not null) ValidateProperty(propertyName);
        return changed;
    }

    protected void ValidateProperty(string propertyName)
    {
        if (!_validators.TryGetValue(propertyName, out var validator)) return;

        var error = validator();
        if (string.IsNullOrWhiteSpace(error)) _errors.Remove(propertyName);
        else _errors[propertyName] = error;

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
        RefreshCommands();
    }

    protected void ValidateAll()
    {
        foreach (var name in _validators.Keys.ToArray()) ValidateProperty(name);
    }

    protected void Revalidate(params string[] propertyNames)
    {
        foreach (var name in propertyNames) ValidateProperty(name);
    }

    /// <summary>Проверка ошибок только по указанным свойствам (для CanExecute конкретной кнопки).</summary>
    protected bool HasErrorsFor(params string[] propertyNames)
        => propertyNames.Any(name => _errors.ContainsKey(name));
}