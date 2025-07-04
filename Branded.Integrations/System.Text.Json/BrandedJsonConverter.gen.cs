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

#nullable disable

[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(global::System.Text.Json.Serialization.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.System.Text.Json.Int32BrandedJsonConverter<,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        },
    },
    OnlyForInnerTypes = new[]
    {
        typeof(global::System.Int32)
    }
)]
[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(global::System.Text.Json.Serialization.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.System.Text.Json.Int64BrandedJsonConverter<,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        },
    },
    OnlyForInnerTypes = new[]
    {
        typeof(global::System.Int64)
    }
)]
[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(global::System.Text.Json.Serialization.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.System.Text.Json.UInt32BrandedJsonConverter<,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        },
    },
    OnlyForInnerTypes = new[]
    {
        typeof(global::System.UInt32)
    }
)]
[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(global::System.Text.Json.Serialization.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.System.Text.Json.UInt64BrandedJsonConverter<,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        },
    },
    OnlyForInnerTypes = new[]
    {
        typeof(global::System.UInt64)
    }
)]
[assembly: global::Branded.SourceGeneratorCustomAttribute(
    typeof(global::System.Text.Json.Serialization.JsonConverter),
    ConstructorArguments = new object[]
    {
        new[]
        {
            typeof(global::Branded.Integrations.System.Text.Json.StringBrandedJsonConverter<,>),
            typeof(global::Branded.BrandedTypePlaceholder),
            typeof(global::Branded.BrandedConverterTypePlaceholder)
        },
    },
    OnlyForInnerTypes = new[]
    {
        typeof(global::System.String)
    }
)]

namespace Branded.Integrations.System.Text.Json
{

    internal sealed class Int32BrandedJsonConverter<TBranded, TFactory> : global::System.Text.Json.Serialization.JsonConverter<TBranded>
        where TFactory : global::Branded.IBrandedValueConverter<TBranded, global::System.Int32>, new()
    {
        private readonly TFactory factory = new TFactory();

        public override TBranded Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            return factory.Wrap(reader.GetInt32());
        }

        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            TBranded value,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            writer.WriteNumberValue(factory.Unwrap(value));
        }
    }

    internal sealed class Int64BrandedJsonConverter<TBranded, TFactory> : global::System.Text.Json.Serialization.JsonConverter<TBranded>
        where TFactory : global::Branded.IBrandedValueConverter<TBranded, global::System.Int64>, new()
    {
        private readonly TFactory factory = new TFactory();

        public override TBranded Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            return factory.Wrap(reader.GetInt64());
        }

        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            TBranded value,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            writer.WriteNumberValue(factory.Unwrap(value));
        }
    }

    internal sealed class UInt32BrandedJsonConverter<TBranded, TFactory> : global::System.Text.Json.Serialization.JsonConverter<TBranded>
        where TFactory : global::Branded.IBrandedValueConverter<TBranded, global::System.UInt32>, new()
    {
        private readonly TFactory factory = new TFactory();

        public override TBranded Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            return factory.Wrap(reader.GetUInt32());
        }

        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            TBranded value,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            writer.WriteNumberValue(factory.Unwrap(value));
        }
    }

    internal sealed class UInt64BrandedJsonConverter<TBranded, TFactory> : global::System.Text.Json.Serialization.JsonConverter<TBranded>
        where TFactory : global::Branded.IBrandedValueConverter<TBranded, global::System.UInt64>, new()
    {
        private readonly TFactory factory = new TFactory();

        public override TBranded Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            return factory.Wrap(reader.GetUInt64());
        }

        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            TBranded value,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            writer.WriteNumberValue(factory.Unwrap(value));
        }
    }

    internal sealed class StringBrandedJsonConverter<TBranded, TFactory> : global::System.Text.Json.Serialization.JsonConverter<TBranded>
        where TFactory : global::Branded.IBrandedValueConverter<TBranded, global::System.String>, new()
    {
        private readonly TFactory factory = new TFactory();

        public override TBranded Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            return factory.Wrap(reader.GetString());
        }

        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            TBranded value,
            global::System.Text.Json.JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(factory.Unwrap(value));
        }
    }

}