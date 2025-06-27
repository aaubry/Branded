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

namespace Branded;

/// <summary>
/// Allows to perform registration of branded types, or similar operations,
/// without having to write the related type names.
/// This is intended to be used with the Dispatch method that is generated
/// in all branded types.
/// </summary>
public interface IBrandedTypeDispatcher
{
    /// <summary>
    /// Performs any desired action related to a branded type.
    /// </summary>
    /// <typeparam name="TBranded">The type of the branded type.</typeparam>
    /// <typeparam name="TInner">The inner type of the branded type.</typeparam>
    /// <typeparam name="TConverter">The branded type's converter type.</typeparam>
    void Dispatch<TBranded, TInner, TConverter>()
        where TConverter : IBrandedValueConverter<TBranded, TInner>, new();
}
