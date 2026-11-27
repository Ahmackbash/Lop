namespace Library.App.Services;

/// <summary>Нарушение бизнес-правила: показывается пользователю как предупреждение.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}