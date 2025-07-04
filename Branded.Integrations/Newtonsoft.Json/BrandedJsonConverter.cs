// MIT License
// 
// Copyright (c) 2025 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(Newtonsoft.Json.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.Newtonsoft.Json.BrandedJsonConverter<,,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedInnerTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        }
    }
)]

namespace Branded.Integrations.Newtonsoft.Json
{
    internal sealed class BrandedJsonConverter<TIdentifier, TInner, TConverter> : global::Newtonsoft.Json.JsonConverter<TIdentifier>
        where TIdentifier : struct
        where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
    {
        private static readonly TConverter Converter = new TConverter();

        public override TIdentifier ReadJson(
            global::Newtonsoft.Json.JsonReader reader,
            global::System.Type objectType,
            TIdentifier existingValue,
            bool hasExistingValue,
            global::Newtonsoft.Json.JsonSerializer serializer
        )
        {
            var id = serializer.Deserialize(reader, typeof(TInner))
                ?? throw new global::System.InvalidCastException($"Cannot convert null value to {typeof(TIdentifier).Name}");

            return Converter.Wrap((TInner)id);
        }

        public override void WriteJson(global::Newtonsoft.Json.JsonWriter writer, TIdentifier value, global::Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, Converter.Unwrap(value));
        }
    }
}