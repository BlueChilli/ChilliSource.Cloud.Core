using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public interface IPagedList<T> : IList<T>
    {
        int CurrentPage { get; set; }
        int PageCount { get; set; }
        int PageSize { get; set; }
        int TotalCount { get; set; }
    }
}
