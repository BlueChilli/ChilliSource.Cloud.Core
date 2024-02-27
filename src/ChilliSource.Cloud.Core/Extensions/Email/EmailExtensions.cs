using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Email
{
    public static class EmailExtensions
    {
        /// <summary>
        /// Check that a email address is syntactically valid
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(this string emailAddress)
        {
            if (String.IsNullOrEmpty(emailAddress) || emailAddress.Length > 256 || emailAddress.Length < 6 || emailAddress.EndsWith(".") || emailAddress.Contains("..")) return false;

            var domain = emailAddress.GetEmailAddressDomain();

            if (String.IsNullOrEmpty(domain) || domain.Length > 200 || domain.Length < 4) return false;

            //check local part only contains valid characters 
            var domainRegex = new Regex("\\A([a-z0-9]+(-[a-z0-9]+)*\\.)+[a-z]{2,}\\Z", RegexOptions.IgnoreCase);
            if (!domainRegex.IsMatch(domain)) return false;

            var local = emailAddress.GetEmailAddressLocalPart();

            if (String.IsNullOrEmpty(local) || local.Length > 64) return false;

            //check local part only contains valid ascii characters 
            var regex = new Regex("^[\\da-zA-Z!#$%&'*+-\\/=?^_`{|}~]*$");
            if (!regex.IsMatch(local)) return false;

            try
            {
                var mailAddress = new MailAddress(emailAddress);
            }
            catch (FormatException)
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// Return the domain (content after the @ symbol) of an email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns>Domain</returns>
        public static string GetEmailAddressDomain(this string emailAddress)
        {
            return emailAddress.Contains('@') ? emailAddress.Split('@')[1] : null;
        }

        /// <summary>
        /// Return the local part (content before the @ symbol) of an email address
        /// </summary>
        public static string GetEmailAddressLocalPart(this string emailAddress)
        {
            return emailAddress.Contains('@') ? emailAddress.Split('@')[0] : null;
        }

    }
}
