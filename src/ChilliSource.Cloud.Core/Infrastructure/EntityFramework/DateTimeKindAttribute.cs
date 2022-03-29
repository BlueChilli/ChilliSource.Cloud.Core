using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Reflection;

namespace ChilliSource.Cloud.Core.EntityFramework
{
    /// <summary>
    /// Specify a datetime kind for a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        /// <summary>
        /// Specifies this property has a datetime kind.
        /// </summary>
        /// <param name="kind">The kind value of the property.</param>
        public DateTimeKindAttribute(DateTimeKind kind = DateTimeKind.Utc)
        {
            Kind = kind;
        }

        public DateTimeKind Kind { get; private set; }

        /// <summary>
        /// Interate all entities checking for  DateTimeKind attributes and setting conversions for those found.
        /// Shortcut for modelBuilder.Entity&lt;T&gt;().Property(x => x.CreatedOn).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        /// </summary>
        /// <param name="builder">The modelbuilder</param>
        public static void OnModelCreating(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                {
                    var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
                    var kind = memberInfo?.GetCustomAttribute<DateTimeKindAttribute>();
                    if (kind == null) continue;

                    var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                        v => v, v => DateTime.SpecifyKind(v, kind.Kind));

                    property.SetValueConverter(dateTimeConverter);
                }
        }
    }
}

