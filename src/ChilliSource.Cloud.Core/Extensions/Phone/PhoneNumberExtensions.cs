using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Phone
{
    public static class PhoneNumberExtensions
    {

        /// <summary>
        /// convert phone number object to string with given format
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(this PhoneNumber phone, PhoneNumberFormat format)
        {
            if (phone == null)
            {
                return null;
            }

            var util = PhoneNumberUtil.GetInstance();

            return util.Format(phone, format).RemoveSpaces();
        }

        /// <summary>
        /// format phone number string to given format
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="region"></param>
        /// <param name="format"></param>
        /// <param name="phoneTypesToCheck"></param>
        /// <returns></returns>
        public static string FormatNumber(this string mobile, string region, PhoneNumberFormat format, params PhoneNumberType[] phoneTypesToCheck)
        {
            return GetPhoneNumber(mobile, region, format, phoneTypesToCheck);
        }
     
        /// <summary>
        /// validate phone number string whether it's a valid phone number or not
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="region"></param>
        /// <param name="phoneTypesToCheck"></param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(this string mobile, string region, params PhoneNumberType[] phoneTypesToCheck)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(region);
            }

            if (String.IsNullOrWhiteSpace(mobile)) return false;

            var phone = mobile.GetPhoneNumber(region, PhoneNumberFormat.E164, phoneTypesToCheck);
            return !String.IsNullOrWhiteSpace(phone);
        }

        /// <summary>
        /// convert to international phone number format ie adds + in front of a number
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string GetInternationalMobileFormat(this string mobile)
        {

            if (!String.IsNullOrWhiteSpace(mobile) && !mobile.StartsWith("+"))
            {
                return String.Format("+{0}", mobile);
            }

            return mobile;
        }

        private static PhoneNumber ExtractPhoneNumber(this string number, string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(region);
            }

            if (!String.IsNullOrWhiteSpace(number))
            {
                try
                {
                    var num = PhoneNumberUtil.ExtractPossibleNumber(number);

                    if (!String.IsNullOrWhiteSpace(num))
                    {
                        var util = PhoneNumberUtil.GetInstance();

                        var phone = util.Parse(num, region);

                        return phone;

                    }
                }
                catch (Exception e)
                {

                }

            }


            return null;
        }

        private static string GetPhoneNumber(this string mobile, string region, PhoneNumberFormat format, params PhoneNumberType[] phoneTypesToCheck)
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

            if (phoneTypesToCheck != null && phoneTypesToCheck.Length > 0 && phoneTypesToCheck.Contains(phoneType))
            {
                return phoneNumber.FormatPhoneNumber(format);
            }

            return null;

        }


        private static string RemoveSpaces(this string input)
        {
            return String.IsNullOrWhiteSpace(input) ? input : Regex.Replace(input, @"[^0-9]+", string.Empty);
        }


    }
}
