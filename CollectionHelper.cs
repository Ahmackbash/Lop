using System.Collections.ObjectModel;

namespace Library.App.Infrastructure;

/// <summary>Вспомогательные операции над ObservableCollection.</summary>
public static class CollectionHelper
{
    /// <summary>Заменяет содержимое коллекции, не пересоздавая её (привязки сохраняются).</summary>
    public static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items) target.Add(item);
    }
}