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
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestProject")]

namespace Branded;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed partial class SourceGeneratorConventionsAttribute : Attribute
{
    [Obsolete("This member exists for testing purposes. Do not use it", error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal int __DoNotUse
    {
        get => 0;
        set { }
    }
}