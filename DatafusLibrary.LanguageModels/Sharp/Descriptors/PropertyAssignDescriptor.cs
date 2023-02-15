﻿using DatafusLibrary.Core.Extensions;

namespace DatafusLibrary.LanguageModels.Sharp.Descriptors;

public class PropertyAssignDescriptor
{
    public string Destination { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public ParameterDescriptor ToCamelCase()
    {
        return new ParameterDescriptor
        {
            Name = Source.ToCamelCase(),
            Type = Type
        };
    }
}