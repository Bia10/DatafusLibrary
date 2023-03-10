using System.Globalization;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.LanguageModels.Sharp;

public class BasicClass
{
    public string GenerationTime => DateTime.Now.ToString(CultureInfo.InvariantCulture);

    public List<string> Usings { get; set; } = new();

    public string Namespace { get; set; } = string.Empty;

    public string ClassModifier { get; set; } = "public";

    public string ClassAccessModifier { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string ClassBase { get; set; } = string.Empty;

    public string BaseClassAccessor => string.IsNullOrEmpty(ClassBase) ? string.Empty : ":";

    public List<PropertyDescriptor> Properties { get; set; } = new();

    public List<ParameterDescriptor> ConstructorParameters { get; set; } = new();

    public List<PropertyAssignDescriptor> InjectedProperties { get; set; } = new();

    public IEnumerable<ParameterDescriptor> Constructor => ConstructorParameters
        .Concat(InjectedProperties.Select(x => x.ToCamelCase()));

    public string BaseConstructorAccessor => string.IsNullOrEmpty(ClassBase) ? string.Empty : ":";

    public string BaseConstructorDeclaration => string.IsNullOrEmpty(ClassBase) ? string.Empty : "base";

    public string BaseConstructorOpeningBrace => string.IsNullOrEmpty(ClassBase) ? string.Empty : "(";

    public IEnumerable<string> BaseConstructor => ConstructorParameters
        .Select(x => x.Name);

    public string BaseConstructorClosingBrace => string.IsNullOrEmpty(ClassBase) ? string.Empty : ")";
}