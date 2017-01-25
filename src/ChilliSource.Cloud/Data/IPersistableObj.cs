using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data
{
    /// <summary>
    /// Interface for persistable objects that have a simple primary key (int).
    /// </summary>
    public interface IPersistableObj
    {
        /// <summary>
        /// Simple primary key.
        /// </summary>
        int Id { get; set; }
    }
}
