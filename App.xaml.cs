using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Library.App.Models;
using Library.App.Services;
using Library.App.ViewModels;
using Library.App.Views;
using Library.App.Infrastructure;

namespace Library.App;

/// <summary>
/// Корень композиции: вручную собирает сервисы и ViewModel (сторонние DI/MVVM-фреймворки не используются).
/// </summary>
public partial class App : Application
{
	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Любое необработанное исключение UI-потока не должно закрывать приложение.
		DispatcherUnhandledException += (_, args) =>
		{
			MessageBox.Show($"Непредвиденная ошибка: {args.Exception.Message}",
				"Библиотека", MessageBoxButton.OK, MessageBoxImage.Error);
			args.Handled = true;
		};

		var paths = new AppPaths();
		paths.EnsureDirectories();

		var backups = new BackupService(paths);
		var storageOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // кириллица без \uXXXX
		};

		var repository = new LibraryRepository(
			new JsonFileStore<Book>(paths.BooksFile, backups, storageOptions),
			new JsonFileStore<Reader>(paths.ReadersFile, backups, storageOptions),
			new JsonFileStore<Loan>(paths.LoansFile, backups, storageOptions),
			paths);

		var loanService = new LoanService();
		var reportService = new ReportService(loanService);
		var importer = new CsvCatalogImporter();
		var exporter = new JsonReportExporter();
		var dialogs = new DialogService();
		var state = new ApplicationState();

		var workspace = new LibraryWorkspace(repository, importer, paths, state, dialogs);

		var catalog = new CatalogViewModel(workspace, loanService, importer, dialogs, paths, state);
		var readers = new ReadersViewModel(workspace, dialogs, state);
		var loans = new LoansViewModel(workspace, loanService, dialogs, state);
		var reports = new ReportsViewModel(workspace, reportService, exporter, dialogs, state);
		var main = new MainViewModel(workspace, state, dialogs, catalog, readers, loans, reports);

		var window = new MainWindow { DataContext = main };
		MainWindow = window;
		window.Show();

		await main.InitializeAsync();
	}
}