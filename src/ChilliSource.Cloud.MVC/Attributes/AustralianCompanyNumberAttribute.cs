using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Australian Company Number validation (ACN)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AustralianCompanyNumberAttribute : CheckSumNumberAttribute
    {
        private readonly int[] _acnWeight = { 8, 7, 6, 5, 4, 3, 2, 1 }; 
        private readonly Regex _acnRegex = new Regex("\\d{9}");

        public AustralianCompanyNumberAttribute() : base("acn") { }

        public override bool IsValid(object val)
        {
            var number = val as string;

            if (String.IsNullOrWhiteSpace(number))
                return true;

            var acn = RemoveWhitespacesInBetween(number);

            if (!_acnRegex.IsMatch(acn))
                return false;

            if (!IsValidCheckSum(number))
                return false;

            return true;
        }

        private bool IsValidCheckSum(string number)
        {
            var isValid = true;
            try
            {
                var remainder = 0;
                var calculatedCheckDigit = 0;

                // Sum the multiplication of all the digits and weights
                var sum = _acnWeight.Select((t, i) => Convert.ToInt32(number.Substring(i, 1))*t).Sum();

                // Divide by 10 to obtain remainder
                remainder = sum%10;

                // Complement the remainder to 10
                calculatedCheckDigit = (10 - remainder == 10) ? 0 : (10 - remainder);

                // Compare the calculated check digit with the actual check digit
                isValid = (calculatedCheckDigit == Convert.ToInt32(number.Substring(8, 1)));
            }
            catch
            {
                isValid = false;
            }
            return isValid;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("The {0} field contains invalid ACN", name);
        }
    }
}