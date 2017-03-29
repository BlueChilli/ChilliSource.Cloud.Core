using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Allows an MVC attribute to become a property binder provider.
    /// </summary>
    public interface IPropertyBinderProvider
    {
        /// <summary>
        /// Creates a binder that implements the IPropertyBinder interface.
        /// </summary>
        IPropertyBinder CreateBinder();
    }
}
