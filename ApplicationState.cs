using System.Threading;
using Library.App.Models;

namespace Library.App.Infrastructure;

/// <summary>
/// Общее состояние занятости и источник CancellationToken для текущей операции.
/// Одна операция одновременно — команды на это время блокируются.
/// </summary>
public sealed class ApplicationState : ObservableModel
{
    private CancellationTokenSource? _source;
    private bool _isBusy;
    private string _statusMessage = "Готово";

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value)) OnPropertyChanged(nameof(CanCancel));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        internal set => SetProperty(ref _statusMessage, value);
    }

    public bool CanCancel => _isBusy && _source is { IsCancellationRequested: false };

    public CancellationToken BeginOperation(string statusMessage)
    {
        if (IsBusy) throw new InvalidOperationException("Другая операция уже выполняется.");

        _source = new CancellationTokenSource();
        StatusMessage = statusMessage;
        IsBusy = true;
        OnPropertyChanged(nameof(CanCancel));
        return _source.Token;
    }

    public void EndOperation(string statusMessage)
    {
        _source?.Dispose();
        _source = null;
        IsBusy = false;
        StatusMessage = statusMessage;
        OnPropertyChanged(nameof(CanCancel));
    }

    public void CancelCurrentOperation()
    {
        _source?.Cancel();
        StatusMessage = "Отмена операции...";
        OnPropertyChanged(nameof(CanCancel));
    }
}