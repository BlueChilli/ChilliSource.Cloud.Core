using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Collections
{
    public static class CollectionExtensions
    {
        public static T[] EmptyArray<T>() { return ArrayEmpty<T>.Value; }
        public static IList<T> EmptyList<T>() { return ArrayEmpty<T>.ValueAsList; }
    }

    internal static class ArrayEmpty<T>
    {
        public static readonly T[] Value = new T[0] { };
        public static readonly IList<T> ValueAsList = Value;
    }
}
