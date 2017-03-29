using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Describes a custom order attribute in an Enum value.
    /// e.g :
    /// public enum ResponseToEvent
    /// {
    /// [Order(1)]
    ///  Going,
    /// [Order(3)]
    ///  NotGoing,
    /// [Order(2)]
    /// Maybe
    /// }
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public class OrderAttribute : Attribute
    {
        public readonly int Order;
        public OrderAttribute(int order)
        {
            Order = order;
        }
    }

}
