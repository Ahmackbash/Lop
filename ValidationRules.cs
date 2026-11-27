using System.Text.RegularExpressions;

namespace Library.App.Services;

/// <summary>Общие правила проверки полей: используются и в формах, и при импорте CSV.</summary>
public static class ValidationRules
{
    public static bool IsIsbnFormatValid(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;
        if (isbn.Any(symbol => !char.IsDigit(symbol) && symbol != '-' && symbol != ' ')) return false;

        var digits = isbn.Count(char.IsDigit);
        return digits is 10 or 13;
    }

    public static bool IsPhoneValid(string? phone)
        => !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^\+?[\d\(\)\-\s]{7,20}$");

    public static bool IsTicketValid(string? ticket)
        => !string.IsNullOrWhiteSpace(ticket) && Regex.IsMatch(ticket, @"^[A-Za-zА-Яа-яЁё0-9\-]{3,20}$");

    public static bool IsYearValid(int year) => year >= 1400 && year <= DateTime.Today.Year + 1;
}