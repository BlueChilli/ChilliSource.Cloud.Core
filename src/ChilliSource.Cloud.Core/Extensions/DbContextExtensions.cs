using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ChilliSource.Core.Extensions;

#if NET_4X
using System.Data.Entity;
using System.Data.Entity.Migrations;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace ChilliSource.Cloud.Core
{
    public static class DbContextExtensions
    {
        public static object[] GetPrimaryKeys<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            if (entity == null)
                return ArrayExtensions.EmptyArray<object>();

            var metadata = PrimaryKeysMetadataFactory<TEntity>.GetForContext(context);
            return metadata.GetPrimaryKeys(entity);
        }

        public static Expression<Func<TEntity, bool>> GetPrimaryKeysFilter<TEntity>(this DbContext context, object[] keyValues) where TEntity : class
        {
            var metadata = PrimaryKeysMetadataFactory<TEntity>.GetForContext(context);
            return metadata.GetKeysFilter(keyValues);
        }
    }
}
