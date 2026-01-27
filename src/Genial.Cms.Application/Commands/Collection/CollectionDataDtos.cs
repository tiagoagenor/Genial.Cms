#nullable enable
using System.Collections.Generic;

namespace Genial.Cms.Application.Commands.Collection;

// FILE
public sealed class FileDataDto
{
    public long? MaxFileSize { get; set; }
    public List<string> MiniTypes { get; set; } = new();
    public bool Required { get; set; }
}

// INPUT
public sealed class InputDataDto
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Validation { get; set; }
    public bool Required { get; set; }
}

// TEXT
public sealed class TextDataDto
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Validation { get; set; }
    public bool Required { get; set; }
}

// NUMBER
public sealed class NumberDataDto
{
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public bool Required { get; set; }
    public bool AllowDecimals { get; set; }
}

// EMAIL
public sealed class EmailDataDto
{
    public bool Required { get; set; }
}

// SELECT
public sealed class SelectDataDto
{
    public List<CollectionFieldOptionDto> Options { get; set; } = new();
    public bool Required { get; set; }
}

// RADIO
public sealed class RadioDataDto
{
    public List<CollectionFieldOptionDto> Options { get; set; } = new();
    public bool Required { get; set; }
}

// BOOL
public sealed class BoolDataDto
{
    public bool Required { get; set; }
}

// CHECKBOX
public sealed class CheckboxDataDto
{
    public List<CollectionFieldOptionDto> Options { get; set; } = new();
    public bool Required { get; set; }
}

// RANGE
public sealed class RangeDataDto
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public bool Required { get; set; }
}

// COLOR
public sealed class ColorDataDto
{
    public bool Required { get; set; }
}

public sealed class CollectionFieldOptionDto
{
    public string Label { get; set; } = default!;
    public string Value { get; set; } = default!;
}
