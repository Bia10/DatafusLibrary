using System.Globalization;
using DatafusLibrary.SourceGenerators.Models.Sharp.Descriptors;

namespace DatafusLibrary.SourceGenerators.Models.Sharp;

public class BasicClass
{
    public string GenerationTime => DateTime.Now.ToString(CultureInfo.InvariantCulture);

    public List<string> Usings { get; set; } = new();

    public string Namespace { get; set; } = string.Empty;

    public string ClassAttributes { get; set; } = string.Empty;

    public string ClassAccessModifier { get; set; } = "public";

    public string ClassName { get; set; } = string.Empty;

    public string ClassBase { get; set; } = string.Empty;

    public string BaseClassAccessor => string.IsNullOrEmpty(ClassBase) ? string.Empty : ":";

    public List<PropertyDescriptor> Properties { get; set; } = new();

    public List<ParameterDescriptor> ConstructorParameters { get; set; } = new();

    public List<PropertyAssignDescriptor> InjectedProperties { get; set; } = new();

    public IEnumerable<ParameterDescriptor> Constructor => ConstructorParameters;

    public string BaseConstructorAccessor => string.IsNullOrEmpty(ClassBase) ? string.Empty : ":";

    public string BaseConstructorDeclaration => string.IsNullOrEmpty(ClassBase) ? string.Empty : "base";

    public string BaseConstructorOpeningBrace => string.IsNullOrEmpty(ClassBase) ? string.Empty : "(";

    public IEnumerable<string> BaseConstructor => string.IsNullOrEmpty(ClassBase)
        ? new List<string>()
        : ConstructorParameters.Select(x => x.Name);

    public string BaseConstructorClosingBrace => string.IsNullOrEmpty(ClassBase) ? string.Empty : ")";
}