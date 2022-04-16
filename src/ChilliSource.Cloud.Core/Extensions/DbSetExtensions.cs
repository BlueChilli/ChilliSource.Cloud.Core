using ChilliSource.Cloud.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ChilliSource.Cloud.Core
{
    public static class DbSetExtensions
    {

        /// <summary>
        /// Syncronize a set of entities via entity key while checking access to foreign key for new entries. 
        /// </summary>
        /// <typeparam name="TForeignEntity">The foreign entity eg Colour</typeparam>
        /// <typeparam name="TManyEntity">The nested many entity eg CarColours</typeparam>
        /// <param name="collectionTable">Table which stores the collection of foreign key</param>
        /// <param name="modelIds">Ids to be syncronised</param>
        /// <param name="manyEntitySet">Current entity state eg collection of car colours eg List of CarColour</param>
        /// <param name="manyEntityKey">Property of many entity key eg get => CarColour.ColourId</param>
        /// <param name="setManyEntityKey">Set the new value of the many entity eg set => CarColour.ColourId </param>
        /// <param name="isAuthorisedForeignEntity">Check for Foreign Entity authorisation. eg IQueryable Colour by primary key</param>
        /// <returns></returns>
        public static ServiceResult SynchronizeCollection<TManyEntity, TForeignEntity>(
            this DbSet<TManyEntity> collectionTable,
            List<int> modelIds,
            ICollection<TManyEntity> manyEntitySet,
            Func<TManyEntity, int> manyEntityKey,
            Action<TManyEntity, int> setManyEntityKey,
            Func<int, IQueryable<TForeignEntity>> isAuthorisedForeignEntity
            )
            where TForeignEntity : class, new()
            where TManyEntity : class, new()
        {

            if (manyEntitySet == null) throw new ArgumentNullException("manyEntitySet", "Many Entity Set must not be null");
            if (modelIds == null) modelIds = new List<int>();
            foreach (var id in modelIds)
            {
                if (!manyEntitySet.Any(x => manyEntityKey(x) == id))
                {
                    if (isAuthorisedForeignEntity(id).Any())
                    {
                        var entityItem = new TManyEntity();
                        setManyEntityKey(entityItem, id);
                        manyEntitySet.Add(entityItem);
                    }
                    else
                    {
                        return ServiceResult.AsError($"Entity with id {id} was not found or is not authorised");
                    }
                }
            }
            collectionTable.RemoveRange(manyEntitySet.Where(x => !modelIds.Any(id => id == manyEntityKey(x))));

            return ServiceResult.AsSuccess();
        }
    }
}
