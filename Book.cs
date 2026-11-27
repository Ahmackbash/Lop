namespace Library.App.Models;

/// <summary>Книга каталога. ISBN — уникальный ключ книги.</summary>
public sealed class Book : ObservableModel
{
	private string _author = string.Empty;
	private string _title = string.Empty;
	private string _isbn = string.Empty;
	private int _year;
	private string _genre = string.Empty;
	private int _totalCopies;

	public string Author { get => _author; set => SetProperty(ref _author, value); }
	public string Title { get => _title; set => SetProperty(ref _title, value); }
	public string Isbn { get => _isbn; set => SetProperty(ref _isbn, value); }
	public int Year { get => _year; set => SetProperty(ref _year, value); }
	public string Genre { get => _genre; set => SetProperty(ref _genre, value); }

	/// <summary>Всего экземпляров в фонде (свободные считаются по выдачам).</summary>
	public int TotalCopies { get => _totalCopies; set => SetProperty(ref _totalCopies, value); }

	/// <summary>Переносит значения из другого экземпляра, сохраняя ссылку на объект в коллекции.</summary>
	public void CopyFrom(Book other)
	{
		Author = other.Author;
		Title = other.Title;
		Isbn = other.Isbn;
		Year = other.Year;
		Genre = other.Genre;
		TotalCopies = other.TotalCopies;
	}
}