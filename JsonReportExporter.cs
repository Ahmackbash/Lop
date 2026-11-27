using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Library.App.Models;

namespace Library.App.Services;

/// <summary>Стиль имён в выгружаемом JSON.</summary>
public enum JsonNamingStyle
{
    AsIs = 0,
    CamelCase = 1,
    SnakeCase = 2,
    KebabCase = 3
}

/// <summary>Настройки структуры и форматирования выгрузки отчёта.</summary>
public sealed record ReportExportOptions(
    bool WriteIndented, JsonNamingStyle NamingStyle,
    bool IncludeSummary, bool IncludeTopBooks, bool IncludeOverdueReaders, bool IncludeGenreDistribution);

public interface IReportExporter
{
    Task ExportAsync(LibraryReport report, string filePath, ReportExportOptions options, CancellationToken cancellationToken);
}

/// <summary>Выгрузка отчёта в JSON с настраиваемой структурой через JsonSerializerOptions.</summary>
public sealed class JsonReportExporter : IReportExporter
{
    public async Task ExportAsync(LibraryReport report, string filePath, ReportExportOptions options,
        CancellationToken cancellationToken)
    {
        var shaped = report with
        {
            Summary = options.IncludeSummary ? report.Summary : null,
            TopBooks = options.IncludeTopBooks ? report.TopBooks : null,
            OverdueReaders = options.IncludeOverdueReaders ? report.OverdueReaders : null,
            GenreDistribution = options.IncludeGenreDistribution ? report.GenreDistribution : null
        };

        var policy = ResolvePolicy(options.NamingStyle);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.WriteIndented,
            PropertyNamingPolicy = policy,
            DictionaryKeyPolicy = policy,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(shaped, jsonOptions);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8, cancellationToken);
    }

    private static JsonNamingPolicy? ResolvePolicy(JsonNamingStyle style) => style switch
    {
        JsonNamingStyle.CamelCase => JsonNamingPolicy.CamelCase,
        JsonNamingStyle.SnakeCase => JsonNamingPolicy.SnakeCaseLower,
        JsonNamingStyle.KebabCase => JsonNamingPolicy.KebabCaseLower,
        _ => null
    };
}