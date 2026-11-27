using System.Text;
using Library.App.Models;

namespace Library.App.Services;

public sealed record CsvImportResult(IReadOnlyList<Book> Books, IReadOnlyList<string> Errors, int DataRowCount);

public interface ICatalogImporter
{
    Task<CsvImportResult> ImportAsync(string filePath, CancellationToken cancellationToken);
}

/// <summary>
/// Импорт каталога из CSV. Заголовок: Author;Title;ISBN;Year;Genre;Copies
/// (разделитель ';' или ',' определяется автоматически, поддержаны кавычки и русские заголовки).
/// </summary>
public sealed class CsvCatalogImporter : ICatalogImporter
{
    public async Task<CsvImportResult> ImportAsync(string filePath, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8, cancellationToken);
        var books = new List<Book>();
        var errors = new List<string>();

        if (lines.Length == 0)
        {
            errors.Add("Файл пуст.");
            return new CsvImportResult(books, errors, 0);
        }

        var delimiter = lines[0].Count(c => c == ';') >= lines[0].Count(c => c == ',') ? ';' : ',';
        var header = SplitLine(lines[0], delimiter).Select(NormalizeHeader).ToList();

        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < header.Count; i++) index[header[i]] = i;

        string[] required = { "author", "title", "isbn", "year", "genre", "copies" };
        var missing = required.Where(column => !index.ContainsKey(column)).ToList();
        if (missing.Count > 0)
        {
            errors.Add($"В заголовке нет колонок: {string.Join(", ", missing)}. Ожидается Author;Title;ISBN;Year;Genre;Copies.");
            return new CsvImportResult(books, errors, 0);
        }

        var seenIsbn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dataRows = 0;

        for (var i = 1; i < lines.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            dataRows++;
            var fields = SplitLine(lines[i], delimiter);
            string Value(string column) => index[column] < fields.Count ? fields[index[column]].Trim() : string.Empty;

            var author = Value("author");
            var title = Value("title");
            var isbn = Value("isbn");
            var genre = Value("genre");

            if (author.Length == 0 || title.Length == 0 || genre.Length == 0)
            {
                errors.Add($"Строка {i + 1}: не заполнены автор, название или жанр.");
                continue;
            }
            if (!ValidationRules.IsIsbnFormatValid(isbn))
            {
                errors.Add($"Строка {i + 1}: некорректный ISBN «{isbn}» (нужно 10 или 13 цифр).");
                continue;
            }
            if (!seenIsbn.Add(isbn))
            {
                errors.Add($"Строка {i + 1}: ISBN «{isbn}» дублируется в файле.");
                continue;
            }
            if (!int.TryParse(Value("year"), out var year) || !ValidationRules.IsYearValid(year))
            {
                errors.Add($"Строка {i + 1}: некорректный год «{Value("year")}».");
                continue;
            }
            if (!int.TryParse(Value("copies"), out var copies) || copies < 0)
            {
                errors.Add($"Строка {i + 1}: некорректное количество экземпляров «{Value("copies")}».");
                continue;
            }

            books.Add(new Book
            {
                Author = author,
                Title = title,
                Isbn = isbn,
                Year = year,
                Genre = genre,
                TotalCopies = copies
            });
        }

        return new CsvImportResult(books, errors, dataRows);
    }

    private static string NormalizeHeader(string value)
    {
        var header = value.Trim().TrimStart('\uFEFF').ToLowerInvariant();
        return header switch
        {
            "автор" => "author",
            "название" => "title",
            "год" => "year",
            "жанр" => "genre",
            "экземпляры" or "количество" => "copies",
            _ => header
        };
    }

    /// <summary>Разбор строки CSV с учётом кавычек и удвоенных кавычек внутри поля.</summary>
    private static List<string> SplitLine(string line, char delimiter)
    {
        var result = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var symbol = line[i];
            if (symbol == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else inQuotes = !inQuotes;
            }
            else if (symbol == delimiter && !inQuotes)
            {
                result.Add(builder.ToString());
                builder.Clear();
            }
            else builder.Append(symbol);
        }

        result.Add(builder.ToString());
        return result;
    }
}