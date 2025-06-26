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
/// Provides an unified way of wrapping and unwrapping values into branded types.
/// </summary>
public interface IBrandedValueConverter<TBranded, TInner>
{
    /// <summary>
    /// Wraps a <typeparamref name="TInner"/> into a <typeparamref name="TBranded"/>.
    /// </summary>
    TBranded Wrap(TInner value);

    /// <summary>
    /// Unwraps a <typeparamref name="TInner"/> from a <typeparamref name="TBranded"/>.
    /// </summary>
    TInner Unwrap(TBranded value);
}
