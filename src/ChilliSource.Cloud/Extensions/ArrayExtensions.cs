using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public static class ArrayExtensions
    {
        public static T[] EmptyArray<T>() { return EmptyArrayHolder<T>.Value; }
    }

    internal static class EmptyArrayHolder<T>
    {
        public static readonly T[] Value = new T[0] { };
    }
}
