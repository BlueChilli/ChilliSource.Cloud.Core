#if NET_8X

using AutoMapper;
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
        /// Syncronise a set of many entity with a set of many model. Authorisation on FK must be done prior. Use when the many to many entity has associated properties that also need to be synchronised with the many model.
        /// Requires a Mapping between entity and model.
        /// </summary>
        /// <typeparam name="TManyEntity">The nested many entity eg CarColour</typeparam>
        /// <typeparam name="TManyModel">The nested many model eg CarColourEditModel</typeparam>
        /// <param name="collectionTable">Table which stores the collection of mappings between two entities eg Cars and Colours</param>
        /// <param name="manyModelSet">Data to synchronise with</param>
        /// <param name="manyEntitySet">Current entity state eg collection of car colours eg List of CarColour</param>
        /// <param name="modelToEntityMatch">Expression which matches model to entity</param>
        /// <returns></returns>
        public static SynchronizeCollectionResult<TManyEntity> SynchroniseCollection<TManyEntity, TManyModel>(
            this DbSet<TManyEntity> collectionTable,
            IMapper mapper,
            ICollection<TManyModel> manyModelSet,
            ICollection<TManyEntity> manyEntitySet,
            Func<TManyEntity, TManyModel, bool> modelToEntityMatch
            )
            where TManyEntity : class, new()
        {
            var result = new SynchronizeCollectionResult<TManyEntity>();
            if (manyEntitySet == null) throw new ArgumentNullException("manyEntitySet", "Many Entity Set must not be null");
            if (manyModelSet == null) manyModelSet = new List<TManyModel>();
            foreach (var model in manyModelSet)
            {
                var entityItem = manyEntitySet.Where(x => modelToEntityMatch(x, model)).FirstOrDefault();
                if (entityItem == null)
                {
                    entityItem = mapper.Map<TManyEntity>(model);
                    manyEntitySet.Add(entityItem);
                    result.Added.Add(entityItem);
                }
                else
                {
                    mapper.Map(model, entityItem);
                }
            }
            result.Removed = manyEntitySet.Where(x => !manyModelSet.Any(m => modelToEntityMatch(x, m))).ToList();
            if (result.Removed.Any()) collectionTable.RemoveRange(result.Removed);
            return result;
        }

        /// <summary>
        /// Syncronize a set of entities via entity key. Authorisation on FK must be done prior. 
        /// </summary>
        /// <typeparam name="TManyEntity">The nested many entity eg CarColours</typeparam>
        /// <param name="collectionTable">Table which stores the collection of foreign key</param>
        /// <param name="modelIds">Ids to be syncronised</param>
        /// <param name="manyEntitySet">Current entity state eg collection of car colours eg List of CarColour</param>
        /// <param name="manyEntityKey">Property of many entity key eg get => CarColour.ColourId</param>
        /// <param name="setManyEntityKey">Set the new value of the many entity eg set => CarColour.ColourId </param>
        /// <returns></returns>
        public static SynchronizeCollectionResult<TManyEntity> SynchroniseCollectionById<TManyEntity>(
            this DbSet<TManyEntity> collectionTable,
            List<int> modelIds,
            ICollection<TManyEntity> manyEntitySet,
            Func<TManyEntity, int> manyEntityKey,
            Action<TManyEntity, int> setManyEntityKey
            )
            where TManyEntity : class, new()
        {
            var result = new SynchronizeCollectionResult<TManyEntity>();
            if (manyEntitySet == null) throw new ArgumentNullException("manyEntitySet", "Many Entity Set must not be null");
            if (modelIds == null) modelIds = new List<int>();
            foreach (var id in modelIds)
            {
                if (!manyEntitySet.Any(x => manyEntityKey(x) == id))
                {
                    var entityItem = new TManyEntity();
                    setManyEntityKey(entityItem, id);
                    manyEntitySet.Add(entityItem);
                    result.Added.Add(entityItem);
                }
            }
            result.Removed = manyEntitySet.Where(x => !modelIds.Any(id => id == manyEntityKey(x))).ToList();
            if (result.Removed.Any()) collectionTable.RemoveRange(result.Removed);
            return result;
        }
    }

    public class SynchronizeCollectionResult<T>
    {
        public List<T> Added { get; set; } = new List<T>();

        public List<T> Removed { get; set; } = new List<T>();
    }

}
#endif