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

#nullable enable

namespace Branded.Integrations.Dapper
{
    /// <summary>
    /// Configures how a string-branded type is mapped to the database.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Struct)]
    internal sealed class DapperStringType : global::System.Attribute
    {
        public DapperStringType(bool isAnsi)
        {
            IsAnsi = isAnsi;
        }

        public DapperStringType(int length, bool isAnsi) : this(isAnsi)
        {
            Length = length;
        }

        public DapperStringType(int length) : this(length, isAnsi: false)
        {
        }

        public int? Length { get; }
        public bool IsAnsi { get; }
    }

    internal sealed class BrandedTypeRegistry : global::Branded.IBrandedTypeDispatcher
    {
        private BrandedTypeRegistry() { }

        public static readonly BrandedTypeRegistry Instance = new BrandedTypeRegistry();

        private sealed class IdentifierTypeHandler<TIdentifier, TInner, TConverter> : global::Dapper.SqlMapper.ITypeHandler
            where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
        {
            private static readonly TConverter Converter = new TConverter();

            private readonly DapperStringType? dapperStringType;

            public IdentifierTypeHandler()
            {
                if (typeof(TInner) == typeof(string))
                {
                    dapperStringType = (DapperStringType?)global::System.Attribute.GetCustomAttribute(typeof(TIdentifier), typeof(DapperStringType));
                }
            }

            public object Parse(global::System.Type destinationType, object value) => Converter.Wrap((TInner)value)!;

            public void SetValue(global::System.Data.IDbDataParameter parameter, object value)
            {
                if (global::System.DBNull.Value.Equals(value))
                {
                    parameter.Value = global::System.DBNull.Value;
                }
                else
                {
                    parameter.Value = Converter.Unwrap((TIdentifier)value);
                    if (dapperStringType != null)
                    {
                        if (dapperStringType.Length != null)
                        {
                            parameter.Size = dapperStringType.Length.Value;
                            parameter.DbType = dapperStringType.IsAnsi
                                ? global::System.Data.DbType.AnsiStringFixedLength
                                : global::System.Data.DbType.StringFixedLength;
                        }
                        else
                        {
                            parameter.DbType = dapperStringType.IsAnsi
                                ? global::System.Data.DbType.AnsiString
                                : global::System.Data.DbType.String;
                        }
                    }
                }
            }
        }

        public void Dispatch<TIdentifier, TInner, TConverter>() where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
        {
            global::Dapper.SqlMapper.AddTypeHandler(typeof(TIdentifier), new IdentifierTypeHandler<TIdentifier, TInner, TConverter>());
        }
    }
}
