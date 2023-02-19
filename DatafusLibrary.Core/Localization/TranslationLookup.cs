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

        foreach (var fileLine in fileLines)
        {
            if (fileLine.StartsWith("{", StringComparison.Ordinal))
                continue;
            if (fileLine.StartsWith("}", StringComparison.Ordinal))
                continue;

            var splitLine = fileLine.Split(": ", 2);

            var key = Convert.ToInt32(splitLine[0].Replace("\"", ""));
            var value = splitLine[1];

            _internalDictionary?.Add(key, value);
        }
    }

    public string? Get(int key)
    {
        string? result = null;

        if (key < 1)
        {
            Console.WriteLine($"Cannot find translation for key: {key}");
            return result;
        }

        try
        {
            result = _internalDictionary[key];
        }
        catch (Exception e)
        {
            Console.WriteLine($"Cannot find translation for key: {key}", e);
            return result;
        }

        return result;
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