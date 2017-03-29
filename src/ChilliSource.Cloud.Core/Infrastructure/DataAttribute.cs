using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{    
    /// <summary>
    /// Describes additional meta-data information in an Enum value.
    /// This can be obtained by the GetData Extension method (e.g TestEnum.Value.GetData<string>("metadata_xyz"))
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public class DataAttribute : Attribute
    {
        /// <summary>
        /// Meta-data name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Meta-data value. Must be a compile-time constant.
        /// </summary>
        public object Value { get; set; }

        public DataAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Attaches an alias to an Enum value.
    /// The Enum value can be obtained from the alias by using ModelEnumExtensions.GetFromAlias<Enum>(alias) .
    /// </summary>
    public class AliasAttribute : Attribute
    {
        /// <summary>
        /// Alias name
        /// </summary>
        public string Name { get; set; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}
