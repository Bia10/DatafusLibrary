using System.Text;
using System.Text.Json;

namespace DatafusLibrary.Core.Serialization;

public static class Json
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static async Task<T?> DeserializeAsync<T>(string json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException(nameof(json));
        
        var utf8Json = new MemoryStream(Encoding.UTF8.GetBytes(json));

        return await DeserializeAsync<T>(utf8Json, options);
    }

    private static async Task<T?> DeserializeAsync<T>(Stream utf8Json, JsonSerializerOptions? options = null)
    {
        try
        {
            options ??= Options;

            return await JsonSerializer.DeserializeAsync<T>(utf8Json, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}