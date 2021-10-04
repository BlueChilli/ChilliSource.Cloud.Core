using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extension methods for CsvHelper.
    /// </summary>
    public static class CsvHelperExtensions
    {
        /// <summary>
        /// Converts a System.Collections.Generic.List&lt;T&gt; to comma delimiter string by using CsvHelper.
        /// </summary>
        /// <typeparam name="T">The type of the objects to convert.</typeparam>
        /// <param name="items">A System.Collections.Generic.List&lt;T&gt;.</param>
        /// <returns>Comma delimiter string.</returns>
        public static string ToCsvFile<T>(this List<T> items)
        {
            return items.ToCsvFile(new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { ShouldQuote = arg => true });
        }

        /// <summary>
        /// Converts a System.Collections.Generic.List&lt;T&gt; to comma delimiter string based on the CsvHelper.Configuration.CsvConfiguration.
        /// </summary>
        /// <typeparam name="T">The type of the objects to convert.</typeparam>
        /// <param name="items">A System.Collections.Generic.List&lt;T&gt;.</param>
        /// <param name="config">CsvHelper.Configuration.CsvConfiguration</param>
        /// <returns>Comma delimiter string.</returns>
        public static string ToCsvFile<T>(this List<T> items, CsvConfiguration config)
        {
            string csv = "";

            if (items.Any())
            {
                using (var stringWriter = new StringWriter())
                using (var writer = new CsvWriter(stringWriter, config))
                {
                    writer.WriteRecords(items);
                    stringWriter.Flush();

                    return stringWriter.ToString();
                }
            }
            return csv;
        }

        /// <summary>
        /// Converts a System.Collections.Generic.List&lt;T&gt; to comma delimiter string based on the CsvHelper.Configuration.CsvConfiguration.
        /// </summary>
        /// <typeparam name="T">The type of the objects to convert.</typeparam>
        /// <typeparam name="TClassMap">The type of the class map to register</typeparam>
        /// <param name="items">A System.Collections.Generic.List&lt;T&gt;.</param>
        /// <param name="config">CsvHelper.Configuration.CsvConfiguration</param>
        /// <returns>Comma delimiter string.</returns>
        public static string ToCsvFile<T, TClassMap>(this List<T> items, CsvConfiguration config) where TClassMap : ClassMap
        {
            string csv = "";

            if (items.Any())
            {
                using (var stringWriter = new StringWriter())
                using (var writer = new CsvWriter(stringWriter, config))
                {
                    writer.Context.RegisterClassMap<TClassMap>();
                    writer.WriteRecords(items);
                    stringWriter.Flush();

                    return stringWriter.ToString();
                }
            }
            return csv;
        }
    }
}
