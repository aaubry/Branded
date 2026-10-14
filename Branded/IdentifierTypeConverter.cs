using System;
using System.ComponentModel;
using System.Globalization;

namespace Branded;

public sealed class IdentifierTypeConverter<TIdentifier, TInner, TConverter> : TypeConverter
    where TIdentifier : struct
    where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
{
    private static readonly TConverter Converter = new();

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(TInner)
            || sourceType == typeof(string);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string valueAsString)
        {
            var innerValue = (TInner)Convert.ChangeType(valueAsString, typeof(TInner), CultureInfo.InvariantCulture);
            return Converter.Wrap(innerValue);
        }
        else
        {
            return Converter.Wrap((TInner)value);
        }
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(TInner)
            || destinationType == typeof(string);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        var innerValue = Converter.Unwrap((TIdentifier)value!);
        return destinationType == typeof(string)
            ? Convert.ToString(innerValue)
            : innerValue!;
    }
}
