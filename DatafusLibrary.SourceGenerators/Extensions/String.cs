namespace DatafusLibrary.SourceGenerators.Extensions;

public static class String
{
    public static string ToLowerFirstChar(this string input)
    {
        if(string.IsNullOrEmpty(input))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }

    public static string EscapeCSharpKeywords(this string input, bool toLower = false)
    {
        var result = toLower ? input.ToLowerFirstChar() : input;

        if (input.Equals("params", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("operator", StringComparison.OrdinalIgnoreCase))
        {
            result = "@" + result;

            return result;
        }

        return result;
    }
}