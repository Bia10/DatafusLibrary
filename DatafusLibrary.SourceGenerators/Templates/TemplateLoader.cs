using System.Reflection;

namespace DatafusLibrary.SourceGenerators.Templates;

public static class TemplateLoader
{
    private static Assembly GetAssembly(Type? assemblyType)
    {
        var assembly = assemblyType is null 
            ? Assembly.GetExecutingAssembly() 
            : Assembly.GetAssembly(assemblyType);

        return assembly;
    }

    public static string LoadTemplate(string templateName, Type? assemblyType = null)
    {
        var assembly = GetAssembly(assemblyType);

        var resources = assembly.GetManifestResourceNames()
            .Where(resource => resource.EndsWith(templateName, StringComparison.Ordinal))
            .ToList();

        if (!resources.Any())
            throw new InvalidOperationException($"There is no template with name: {templateName} inside assembly: {assembly.FullName}");
        if (resources.Count > 1)
            throw new InvalidOperationException($"There is more then one template with name: {templateName} inside assembly: {assembly.FullName}");

        templateName = resources.Single();

        return ReadTemplate(assembly, templateName);
    }

    private static string ReadTemplate(Assembly assembly, string templateName)
    {
        using var resourceStream = assembly.GetManifestResourceStream(templateName);

        if (resourceStream is null)
            throw new InvalidOperationException(nameof(resourceStream) + "is null cannot open with StreamReader!");

        using var streamReader = new StreamReader(resourceStream);

        return streamReader.ReadToEnd();
    }
}