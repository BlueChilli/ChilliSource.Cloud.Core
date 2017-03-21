using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Extensions.Phone
{
    public static class PhoneNumberExtensions
    {
        public static bool IsValidAustralianMobileNumber(this string number)
        {
            var regEx = new Regex(@"^(?:\+?61|0)4\)?(?:[ -]?[0-9]){7}[0-9]$");

            return regEx.IsMatch(number);
        }

        private static PhoneNumber ExtractPhoneNumber(this string number, string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(region);
            }

            if (!String.IsNullOrWhiteSpace(number))
            {
                var num = PhoneNumberUtil.ExtractPossibleNumber(number);

                if (!String.IsNullOrWhiteSpace(num))
                {
                    var util = PhoneNumberUtil.GetInstance();

                    var phone = util.Parse(num, region);

                    return phone;

                }
            }


            return null;
        }

        public static string GetPhoneNumber(this PhoneNumber phone, PhoneNumberFormat format)
        {
            if (phone == null)
            {
                return null;
            }

            var util = PhoneNumberUtil.GetInstance();

            return util.Format(phone, format).RemoveSpaces();
        }

        public static string GetPhoneNumber(this string mobile, string region, PhoneNumberFormat format)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(region);
            }

            var phoneUtil = PhoneNumberUtil.GetInstance();

            if (String.IsNullOrWhiteSpace(mobile)) return mobile;

            var phoneNumber = mobile.ExtractPhoneNumber(region);

            if (phoneNumber == null)
            {
                return null;
            }

            var phoneType = phoneUtil.GetNumberType(phoneNumber);

            if (mobile.IsValidAustralianMobileNumber())
            {
                return phoneNumber.GetPhoneNumber(format);
            }

            if (phoneType == PhoneNumberType.FIXED_LINE_OR_MOBILE || phoneType == PhoneNumberType.FIXED_LINE || phoneType == PhoneNumberType.MOBILE || phoneType == PhoneNumberType.UNKNOWN)
            {
                return phoneNumber.GetPhoneNumber(format);
            }

            return null;

        }

        public static bool IsValidPhoneNumber(this string mobile, string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(region);
            }

            if (String.IsNullOrWhiteSpace(mobile)) return false;

            var phone = mobile.GetPhoneNumber(region, PhoneNumberFormat.E164);
            return !String.IsNullOrWhiteSpace(phone);
        }

        public static string GetInternationalMobileFormat(this string mobile)
        {

            if (!String.IsNullOrWhiteSpace(mobile) && !mobile.StartsWith("+"))
            {
                return String.Format("+{0}", mobile);
            }

            return mobile;
        }

        private static string RemoveSpaces(this string input)
        {
            return String.IsNullOrWhiteSpace(input) ? input : Regex.Replace(input, @"[^0-9]+", string.Empty);
        }


    }
}
