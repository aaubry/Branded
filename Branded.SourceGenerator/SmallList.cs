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

using System.Collections;

namespace Branded.SourceGenerator
{
    internal sealed class SmallList<T>(T first) : ICollection<T>, IEquatable<SmallList<T>>
    {
        private readonly T first = first;
        private List<T>? others;

        public int Count => others is null ? 1 : 1 + others.Count;

        public void Add(T item)
        {
            others ??= [];
            others.Add(item);
        }

        public bool Contains(T item)
        {
            return Equals(first, item)
                || (others is not null && others.Contains(item));
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex] = first;
            others?.CopyTo(array, arrayIndex + 1);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return first;
            if (others is not null)
            {
                foreach (var other in others)
                {
                    yield return other;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<T>.IsReadOnly => false;

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();


        public bool Equals(SmallList<T> other) => this.SequenceEqual(other);

        public override bool Equals(object obj) => obj is SmallList<T> other && Equals(other);

        public override int GetHashCode() => this.Aggregate(0, (h, i) => HashCode.Combine(h, i is not null ? i.GetHashCode() : 0));
    }

    internal static class SmallList
    {
        public static void Push<T>(ref SmallList<T>? list, T item) where T : notnull
        {
            if (list is null)
            {
                list = new SmallList<T>(item);
            }
            else
            {
                list.Add(item);
            }
        }
    }
}
