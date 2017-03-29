using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Represents a Credit Card Number. 
    /// Returns the card type (Visa, Master, etc)
    /// </summary>
    [Serializable]
    public class CreditCardNumber
    {
        private string _number;
        private string _parsedNumber;
        private CreditCardType _type;

        public CreditCardNumber() { }
        public CreditCardNumber(string number)
        {
            this.Number = number;
        }

        /// <summary>
        /// Gets or sets the Credit Card Number.
        /// </summary>
        public string Number
        {
            get
            {
                return _number;
            }
            set
            {
                _parsedNumber = null;
                _type = CreditCardType.Unknown;

                _number = value;
                if (_number == null)
                    return;

                _parsedNumber = string.Concat(_number.Where(c => c >= '0' && c <= '9'));

                var expressions = new List<KeyValuePair<Regex, CreditCardType>>()
                {
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^4[0-9]{12}(?:[0-9]{3})?$"), CreditCardType.Visa),
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^5[1-5][0-9]{14}$"), CreditCardType.MasterCard),
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^3[47][0-9]{13}$"), CreditCardType.AmericanExpress),
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^3(?:0[0-5]|[68][0-9])[0-9]{11}$"), CreditCardType.DinersClub),
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^6(?:011|5[0-9]{2})[0-9]{12}$"), CreditCardType.Discover),
                    new KeyValuePair<Regex,CreditCardType>(new Regex(@"^(?:2131|1800|35\d{3})\d{11}$"), CreditCardType.JCB)
                };

                _type = expressions.Where(m => m.Key.IsMatch(_parsedNumber)).Select(m => m.Value).FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns only numeric digits.
        /// </summary>
        public string ParsedNumber { get { return _parsedNumber; } }

        /// <summary>
        /// Returns the Credit Card type or CreditCardType.Unknown if the number couldn't be parsed.
        /// </summary>
        public CreditCardType Type { get { return _type; } }
    }

    /// <summary>
    /// Credit Card types
    /// </summary>
    [Serializable]
    public enum CreditCardType
    {
        Unknown = 0,
        Visa,
        MasterCard,
        AmericanExpress,
        DinersClub,
        Discover,
        JCB
    }
}
