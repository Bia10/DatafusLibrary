using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DatafusLibrary.Core.Serialization;

internal static class Json
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    internal static async Task<T?> DeserializeAsync<T>(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var utf8Json = new MemoryStream(Encoding.UTF8.GetBytes(json));

        return await DeserializeAsync<T>(utf8Json, options);
    }

    private static async Task<T?> DeserializeAsync<T>(Stream utf8Json, JsonSerializerOptions? options = null)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(utf8Json, options ?? Options);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw;
        }
        finally
        {
            await utf8Json.DisposeAsync();
        }
    }
}