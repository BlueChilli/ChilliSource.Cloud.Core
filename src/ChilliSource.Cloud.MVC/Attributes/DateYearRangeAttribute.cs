using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Limits a DateTime field to a particular date Range based on the current date + offset.
    /// </summary>
    public class DateYearRangeAttribute : ValidationAttribute, IMetadataAware
    {
        /// <summary>
        /// Offset to calculate the first year of the range. This value should be negative for ranges starting in the past (e.g -10 years in the past)
        /// </summary>
        public int YearLow { get; set; }
        /// <summary>
        /// Total number of years in the range. (e.g 20)
        /// </summary>
        public int YearCount { get; set; }
        /// <summary>
        /// Specifies whether the latest year should be displayed first.
        /// </summary>
        public bool IsReversed { get; set; }

        /// <summary>
        /// Retrieves the first year of the range.
        /// </summary>
        public int YearFrom() { return DateTime.Today.Year + YearLow; }
        /// <summary>
        /// Retrieves the last year of the range.
        /// </summary>
        public int YearTo() { return DateTime.Today.Year + YearLow + YearCount; }

        public DateYearRangeAttribute(int yearLow, int yearCount, bool isReversed = false)
        {
            YearLow = yearLow;
            YearCount = yearCount;
            IsReversed = isReversed;
        }

        public DateYearRangeAttribute(ModelMetadata metadata)
        {
            if (metadata.AdditionalValues.ContainsKey("DateYearRange_YearLow"))
            {
                YearLow = (int)metadata.AdditionalValues["DateYearRange_YearLow"];
                YearCount = (int)metadata.AdditionalValues["DateYearRange_YearCount"];
                IsReversed = (bool)metadata.AdditionalValues["DateYearRange_IsReversed"];
            }
            else
            {
                YearLow = int.MinValue;
            }
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["DateYearRange_YearLow"] = YearLow;
            metadata.AdditionalValues["DateYearRange_YearCount"] = YearCount;
            metadata.AdditionalValues["DateYearRange_IsReversed"] = IsReversed;
        }

        public override bool IsValid(object value)
        {
            if (value == null || !(value is DateTime))
                return true;

            var date = (DateTime)value;

            return YearFrom() <= date.Year && YearTo() >= date.Year;
        }

        public static Dictionary<string, string> GetResponsivePickerOptions(ModelMetadata metadata)
        {
            var result = new Dictionary<string, string>();
            var attribute = new DateYearRangeAttribute(metadata);
            if (attribute.YearLow != int.MinValue)
            {
                result.Add("yearFrom", attribute.YearFrom().ToString());
                result.Add("yearRangeCount", attribute.YearCount.ToString());
                result.Add("isReversed", attribute.IsReversed.ToString());
            }
            return result;
        }
    }
}
