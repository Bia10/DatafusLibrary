namespace DatafusLibrary.Core.Parsers;

public interface IEntityDataParser
{
    Task<T?> GetDataFromJsonAsync<T>(string pathToJson);
}