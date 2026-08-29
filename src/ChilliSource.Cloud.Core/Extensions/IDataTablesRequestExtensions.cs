#if NET10_0_OR_GREATER

using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;


namespace ChilliSource.Cloud.Core;

/// <summary>
/// Extension methods for IDataTablesRequest
/// </summary>
public static class IDataTablesRequestExtensions
{
    /// <summary>
    /// Gets an IActionResult from a IDataTablesRequest and a PagedList of data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IActionResult GetActionResult<T>(this IDataTablesRequest request, PagedList<T> data)
        => request.GetActionResult(data.UnfilteredCount, data.TotalCount, data);
}

#endif
