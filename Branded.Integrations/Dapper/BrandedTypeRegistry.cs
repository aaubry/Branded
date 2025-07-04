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

namespace Branded.Integrations.Dapper
{
    internal sealed class BrandedTypeRegistry : global::Branded.IBrandedTypeDispatcher
    {
        private BrandedTypeRegistry() { }

        public static readonly BrandedTypeRegistry Instance = new BrandedTypeRegistry();

        private sealed class IdentifierTypeHandler<TIdentifier, TInner, TConverter> : global::Dapper.SqlMapper.ITypeHandler
            where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
        {
            private static readonly TConverter Converter = new TConverter();

            public object Parse(global::System.Type destinationType, object value) => Converter.Wrap((TInner)value)!;
            public void SetValue(global::System.Data.IDbDataParameter parameter, object value) => parameter.Value = Converter.Unwrap((TIdentifier)value);
        }

        public void Dispatch<TIdentifier, TInner, TConverter>() where TConverter : IBrandedValueConverter<TIdentifier, TInner>, new()
        {
            global::Dapper.SqlMapper.AddTypeHandler(typeof(TIdentifier), new IdentifierTypeHandler<TIdentifier, TInner, TConverter>());
        }
    }
}
