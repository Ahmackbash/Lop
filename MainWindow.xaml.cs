using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Library.App.ViewModels;

namespace Library.App.Views;

/// <summary>Окно приложения. В code-behind только визуальное событие закрытия.</summary>
public partial class MainWindow : Window
{
    private bool _canClose;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_canClose) return;
        if (DataContext is not MainViewModel viewModel) return;

        // Откладываем закрытие, пока ViewModel решает вопрос сохранения.
        e.Cancel = true;

        var shouldClose = await viewModel.PrepareExitAsync();
        if (!shouldClose) return;

        _canClose = true;
        Dispatcher.InvokeAsync(Close, DispatcherPriority.ApplicationIdle);
    }
}