using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Library.App.Models;

/// <summary>
/// Базовый доменный тип с уведомлением об изменении свойств.
/// Используется только System.ComponentModel (BCL), ссылок на WPF нет.
/// </summary>
public abstract class ObservableModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}