namespace DatafusLibrary.Core.Localization;

public class TranslationLookup
{
    private readonly Dictionary<int, string?> _internalDictionary;
    private const int DefaultSize = 999999;

    public TranslationLookup(int capacity = DefaultSize)
    {
        _internalDictionary = new Dictionary<int, string?>(capacity, EqualityComparer<int>.Default);
    }

    public bool TryAdd(int key, string value)
    {
        if (key < 1)
            throw new ArgumentOutOfRangeException(nameof(key));
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        return _internalDictionary.TryAdd(key, value);
    }

    public bool? TryGetValue(int key, out string? value)
    {
        if (key < 1)
            throw new ArgumentOutOfRangeException(nameof(key));

        if (_internalDictionary.TryGetValue(key, out var internalValue))
        {
            value = internalValue;
            return true;
        }

        value = default;
        return false;
    }
}