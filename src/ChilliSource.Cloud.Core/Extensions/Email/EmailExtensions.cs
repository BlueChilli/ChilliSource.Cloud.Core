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
            if (String.IsNullOrEmpty(emailAddress) || emailAddress.Length > 256 || emailAddress.Length < 6 || emailAddress.EndsWith(".")) return false;

            var domain = emailAddress.GetEmailAddressDomain();

            if (String.IsNullOrEmpty(domain) || domain.Length > 200 || domain.Length < 4 || domain.Contains("..")) return false;

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
        /// Return the domain (content after the @ symbol) of an email address address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns>Domain</returns>
        public static string GetEmailAddressDomain(this string emailAddress)
        {
            return emailAddress.Contains('@') ? emailAddress.Split('@')[1] : null;
        }

    }
}
