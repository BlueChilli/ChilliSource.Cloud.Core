using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Generates a drop down of integers. Min and Max values can be specified. Must be applied on int field.
    /// </summary>
    public class IntegerDropDownAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Minimum integer value
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// Maximum integer value
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Ouput numbers in reverse
        /// </summary>
        public bool IsReverse { get; set; }

        public IntegerDropDownAttribute()
        {
        }

        public IntegerDropDownAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["IntDropDownMin"] = Min;
            metadata.AdditionalValues["IntDropDownMax"] = Max;
            metadata.AdditionalValues["IntDropDownReverse"] = IsReverse;
        }

        public static IntegerDropDownAttribute ResolveAttribute(ModelMetadata metadata, MemberExpression member)
        {
            if (metadata.AdditionalValues.ContainsKey("IntDropDownMin"))
            {
                int min = Convert.ToInt32(metadata.AdditionalValues["IntDropDownMin"]);
                int max = Convert.ToInt32(metadata.AdditionalValues["IntDropDownMax"]);
                bool reverse = Convert.ToBoolean(metadata.AdditionalValues["IntDropDownReverse"]);

                var yearRange = member.Member
                        .GetCustomAttributes(typeof(DateYearRangeAttribute), false)
                        .FirstOrDefault() as DateYearRangeAttribute;
                if (yearRange != null)
                {
                    min = yearRange.YearFrom();
                    max = yearRange.YearTo();
                }

                return new IntegerDropDownAttribute { Min = min, Max = max, IsReverse = reverse };
            }
            return null;
        }
    }
}
