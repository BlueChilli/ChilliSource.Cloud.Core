using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>Returns attribute on member, or NULL if it does not exist.</summary>
        public static T GetAttribute<T>(this MemberInfo mi, bool inherit) where T : Attribute
        {
            return mi.GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();
        }
    }
}
