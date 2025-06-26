// Branded - Branded types for C#
// Copyright (C) 2025 Antoine Aubry

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Branded;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed partial class SourceGeneratorCustomAttributeAttribute : Attribute
{
}

public partial class BrandedTypePlaceholder { }

public partial class BrandedInnerTypePlaceholder { }

public partial class BrandedConverterTypePlaceholder : IBrandedValueConverter<BrandedTypePlaceholder, BrandedInnerTypePlaceholder>
{
    BrandedInnerTypePlaceholder IBrandedValueConverter<BrandedTypePlaceholder, BrandedInnerTypePlaceholder>.Unwrap(BrandedTypePlaceholder value)
    {
        throw new NotSupportedException("This method is not intended to be used");
    }

    BrandedTypePlaceholder IBrandedValueConverter<BrandedTypePlaceholder, BrandedInnerTypePlaceholder>.Wrap(BrandedInnerTypePlaceholder value)
    {
        throw new NotSupportedException("This method is not intended to be used");
    }
}

