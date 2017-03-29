using System;
using System.Text.RegularExpressions;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Australian Business Number validation (ABN)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AustralianBusinessNumberAttribute : CheckSumNumberAttribute
    {
        private readonly int[] _abnWeight = { 10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        private readonly Regex _abnRegex = new Regex("\\d{11}");

        public AustralianBusinessNumberAttribute()
            : base("abn")
        {

        } 

        public override bool IsValid(object value)
        {
            var number = value as string;

            if (String.IsNullOrWhiteSpace(number))
                return true;

            var abn = RemoveWhitespacesInBetween(number).Trim();

            // it's not a 11 digits number
            if (!_abnRegex.IsMatch(abn))
                return false;

            //is valid abn by checking parity
            if (!IsValidCheckSum(abn))
                return false;

            return true;
        }

      

        private bool IsValidCheckSum(string abn)
        {
            var isValid = true;
            try
            {
                var sum = 0;

                for (var i = 0; i < _abnWeight.Length; i++)
                {
                    // Subtract 1 from the first left digit before multiplying against the weight
                    if (i == 0)
                        sum = (Convert.ToInt32(abn.Substring(i, 1)) - 1) * _abnWeight[i];
                    else
                        sum += Convert.ToInt32(abn.Substring(i, 1)) * _abnWeight[i];
                }

                isValid = (sum % 89 == 0);
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("The {0} field contains invalid ABN", name);
        }
    }
}