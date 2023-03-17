namespace DatafusLibrary.Core.Extensions;

public static class ObjectExtensions
{
    public static IEnumerable<string> ToStringArray(this IEnumerable<object?> arrayOfObj, bool includeNulls = false,
        string nullValue = "")
    {
        ArgumentNullException.ThrowIfNull(arrayOfObj);

        if (!includeNulls)
            arrayOfObj = arrayOfObj.Where(obj => obj is not null);

        IEnumerable<string> stringEnumerable = arrayOfObj.Select(obj => (obj ?? nullValue).ToString())!;

        return stringEnumerable.ToArray();
    }
}