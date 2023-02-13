using System.Globalization;

namespace DatafusLibrary.Core.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(this string stringToConvert)
    {
        if (string.IsNullOrEmpty(stringToConvert))
            throw new ArgumentNullException(nameof(stringToConvert));

        if (stringToConvert.Length is 1)
            return stringToConvert.ToLowerInvariant();
        
        var result = char.ToLowerInvariant(stringToConvert[0]) + stringToConvert[1..];

        return result.ToString(CultureInfo.InvariantCulture);
    }

    public static string ToPascalCase(this string stringToConvert)
    {
        if (string.IsNullOrEmpty(stringToConvert)) 
            throw new ArgumentNullException(nameof(stringToConvert));

        if (stringToConvert.Length is 1)
            return stringToConvert.ToUpperInvariant();

        var result = char.ToUpperInvariant(stringToConvert[0]) + stringToConvert[1..];

        return result.ToString(CultureInfo.InvariantCulture);
    }
}