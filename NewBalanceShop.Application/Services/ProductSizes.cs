namespace NewBalanceShop.Application.Services;

/// <summary>Розбір і перевірка розмірів товару (список через кому в полі <c>Product.Size</c>).</summary>
public static class ProductSizes
{
    /// <summary>Доступні розміри товару.</summary>
    public static List<string> Parse(string? sizeField) =>
        (sizeField ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    /// <summary>
    /// Нормалізує обраний розмір: порожній стає єдиним розміром товару, невідомий або відсутній (коли розмірів кілька) — помилка.
    /// </summary>
    /// <exception cref="InvalidOperationException">Розмір не обрано або його немає в товару.</exception>
    public static string Resolve(string productName, string? sizeField, string? requested)
    {
        var sizes = Parse(sizeField);
        if (sizes.Count == 0) return string.Empty;

        var wanted = requested?.Trim() ?? string.Empty;
        if (wanted.Length == 0)
        {
            if (sizes.Count == 1) return sizes[0];
            throw new InvalidOperationException($"Оберіть розмір для '{productName}'.");
        }

        var match = sizes.FirstOrDefault(s => string.Equals(s, wanted, StringComparison.OrdinalIgnoreCase));
        return match ?? throw new InvalidOperationException($"Розміру '{wanted}' немає в товару '{productName}'.");
    }
}
