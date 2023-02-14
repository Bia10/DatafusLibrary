﻿using System.Globalization;
using DatafusLibrary.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.LanguageModels.Sharp;

public class BasicClass
{
    public string GenerationTime => DateTime.Now.ToString(CultureInfo.InvariantCulture);

    public List<string> Usings { get; set; }

    public string Namespace { get; set; }

    public string ClassModifier { get; set; } = "public";

    public string ClassName { get; set; }

    public string ClassBase { get; set; }

    public string BaseClassAccessor => string.IsNullOrEmpty(ClassBase) ? string.Empty : ":";

    public List<PropertyDescriptor> Properties { get; set; } = new List<PropertyDescriptor>();

    public List<ParameterDescriptor> ConstructorParameters { get; set; } = new List<ParameterDescriptor>();

    public List<PropertyAssignDescriptor> InjectedProperties { get; set; } = new List<PropertyAssignDescriptor>();

    public IEnumerable<ParameterDescriptor> Constructor => ConstructorParameters
        .Concat(InjectedProperties.Select(x => x.ToCamelCase()));

    public IEnumerable<string> BaseConstructor => ConstructorParameters
        .Select(x => x.Name);
}