using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    /// <summary>
    /// Extension methods for System.Linq.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Correlates the elements of two sequences based on matching keys, which preserves the unmatched elements from the first sequence, joining them with a NULL in the shape of the second sequence.
        /// </summary>
        /// <typeparam name="TLeft">The type of the first sequence.</typeparam>
        /// <typeparam name="TRight">The type of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the key to match two sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result sequence.</typeparam>
        /// <param name="left">The first sequence.</param>
        /// <param name="right">The second sequence.</param>
        /// <param name="leftKeySelector">A function the select key from the first sequence.</param>
        /// <param name="rightKeySelector">A function the select key from the second sequence.</param>
        /// <param name="resultSelector">A function the select results.</param>
        /// <returns>A System.Collections.Generic.IEnumerable&lt;TResult&gt.</returns>
        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TResult> resultSelector)
        {
            return left.GroupJoin(right, leftKeySelector, rightKeySelector, (l, r) => new { l, r })
                .SelectMany(tuple => tuple.r.DefaultIfEmpty(), (tuple, r) => new { tuple.l, r })
                .Select(tuple => resultSelector(tuple.l, tuple.r));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys, which preserves the unmatched elements from the second sequence, joining them with a NULL in the shape of the first sequence.
        /// </summary>
        /// <typeparam name="TLeft">The type of the first sequence.</typeparam>
        /// <typeparam name="TRight">The type of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the key to match two sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result sequence.</typeparam>
        /// <param name="left">The first sequence.</param>
        /// <param name="right">The second sequence.</param>
        /// <param name="leftKeySelector">A function the select key from the first sequence.</param>
        /// <param name="rightKeySelector">A function the select key from the second sequence.</param>
        /// <param name="resultSelector">A function the select results.</param>
        /// <returns>A System.Collections.Generic.IEnumerable&lt;TResult&gt.</returns>
        public static IEnumerable<TResult> RightOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TResult> resultSelector)
        {
            return right.GroupJoin(left, rightKeySelector, leftKeySelector, (r, l) => new { r, l })
                .SelectMany(tuple => tuple.l.DefaultIfEmpty(), (tuple, l) => new { tuple.r, l })
                .Select(tuple => resultSelector(tuple.l, tuple.r));
        }
    }
}
