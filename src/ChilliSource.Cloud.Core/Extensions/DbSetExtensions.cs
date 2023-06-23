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
        /// Synchronize a set of entities via entity key while checking access to foreign key for new entries. 
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
        public static ServiceResult<SynchronizeCollectionResult> SynchronizeCollection<TManyEntity, TForeignEntity>(
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
            var added = 0;
            foreach (var id in modelIds)
            {
                if (!manyEntitySet.Any(x => manyEntityKey(x) == id))
                {
                    if (isAuthorisedForeignEntity(id).Any())
                    {
                        var entityItem = new TManyEntity();
                        setManyEntityKey(entityItem, id);
                        manyEntitySet.Add(entityItem);
                        added++;
                    }
                    else
                    {
                        return ServiceResult<SynchronizeCollectionResult>.AsError($"Entity with id {id} was not found or is not authorised");
                    }
                }
            }
            var toRemove = manyEntitySet.Where(x => !modelIds.Any(id => id == manyEntityKey(x))).ToList();
            if (toRemove.Any()) collectionTable.RemoveRange(toRemove);

            return ServiceResult<SynchronizeCollectionResult>.AsSuccess(new SynchronizeCollectionResult { Added = added, Removed = toRemove.Count });
        }
    }

    public class SynchronizeCollectionResult
    {
        public int Added { get; set; }
        public int Removed { get; set; }
        public bool Changed => Added > 0 || Removed > 0;
    }
}
