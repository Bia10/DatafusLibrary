using DatafusLibrary.Core.IO;

namespace DatafusLibrary.Core.Localization;

public class TranslationLookup
{
    private readonly Dictionary<int, string?> _internalDictionary;
    private const int DefaultSize = 999999;

    public TranslationLookup(int capacity = DefaultSize)
    {
        _internalDictionary = new Dictionary<int, string?>(capacity, EqualityComparer<int>.Default);
    }

    public async Task LoadTranslationFile(string pathToFile)
    {
        if (string.IsNullOrEmpty(pathToFile))
            throw new ArgumentNullException(nameof(pathToFile));

        var fileLines = await FileReader.ReadAllLinesAsync(pathToFile);

        for (var i = 0; i < fileLines.Length; i ++)
        {
            if (fileLines[i].StartsWith("{", StringComparison.Ordinal))
                continue;
            if (fileLines[i].StartsWith("}", StringComparison.Ordinal))
                continue;

            var value = fileLines[i].Split(": ", 2)[1];

            _internalDictionary?.Add(i, value);
        }
    }

    public string? Get(int key)
    {
        if (key < 1)
            throw new ArgumentOutOfRangeException(nameof(key));

        return _internalDictionary[key];
    }

    public void Add(int key, string value)
    {
        if (key < 1)
            throw new ArgumentOutOfRangeException(nameof(key));
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        _internalDictionary[key] = value;
    }

    public bool TryAdd(int key, string value)
    {
        if (key < 1)
            throw new ArgumentOutOfRangeException(nameof(key));
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        return _internalDictionary.TryAdd(key, value);
    }

    public bool TryGetValue(int key, out string? value)
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