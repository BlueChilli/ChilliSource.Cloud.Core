using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    ///     Extensions for AutoMapper
    /// </summary>
    public static class AutoMapperExtensions
    {
        /// <summary>
        ///     Skips mapping an expression if source value is null, zero or DateTime.MinValue.
        /// </summary>
        /// <param name="expression">Auto mapper configuration expression</param>
        public static void IgnoreIfSourceIsNullOrZero<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> expression)
        {
            expression.Condition((source, destination, sourceMember) => IgnoreIfSourceIsNullOrZero(source, destination, sourceMember));
        }

        static bool IgnoreIfSourceIsNullOrZero<TSource, TDest, TSourceMember>(TSource source, TDest destination, TSourceMember sourceMember)
        {            
            if (sourceMember == null) return false;

            var value = (object)sourceMember;

            if (value is int && ((int)value) == 0) return false;
            if (value is long && ((long)value) == 0) return false;
            if (value is DateTime && ((DateTime)value) == DateTime.MinValue) return false;
            if (value is Enum && Enum.GetUnderlyingType(typeof(TSource)) == typeof(int) && ((int)value) == 0) return false;
            return true;
        }

    }
}
