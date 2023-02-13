using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;

namespace DatafusLibrary.Core.Parsers;

public static class EntityParser
{
    public static (string, string) ParseToStringTuple(Entity entityJson)
    {
        if (entityJson is null)
        {
            throw new ArgumentNullException(nameof(entityJson));
        }

        var defStringArray = entityJson.def?.ToStringArray();
        var dataStringArray = entityJson.data?.ToStringArray();

        if (defStringArray is not null && dataStringArray is not null)
        {
            var defString = string.Join(",", defStringArray);
            defString = defString.Insert(0, "[");
            defString = defString.Insert(defString.Length, "]");

            var dataString = string.Join(",", dataStringArray);
            dataString = dataString.Insert(0, "[");
            dataString = dataString.Insert(dataString.Length, "]");

            return (defString, dataString);
        }

        return (string.Empty, string.Empty);
    }
}