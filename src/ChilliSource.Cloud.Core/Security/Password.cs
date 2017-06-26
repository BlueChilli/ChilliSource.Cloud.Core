using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    //
    // borrowed from: http://joelthecoder.wordpress.com/2009/01/06/generating-random-human-readable-passwords/
    //
    /// <summary>
    /// Represents a random, human-readable password.
    /// </summary>
    public static class Password
    {
        private static readonly IThreadSafeRandom rand = ThreadSafeRandom.Get();

        private static readonly char[] VOWELS = new char[] { 'a', 'e', 'i', 'o', 'u' };
        private static readonly char[] CONSONANTS = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        private static readonly char[] SYMBOLS = new char[] { '*', '?', '/', '\\', '%', '$', '#', '@', '!', '~' };
        private static readonly char[] NUMBERS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Generates a random, human-readable password. 
        /// </summary>
        /// <param name=”numSyllables”>Number of syllables the password will contain</param>
        /// <param name=”numNumeric”>Number of numbers the password will contain</param>
        /// <param name=”numSymbols”>Number of symbols the password will contain</param>
        /// <returns>A random, human-readable password. </returns>
        public static string GenerateHumanReadable(int numSyllables, int numNumeric, int numSymbols)
        {
            StringBuilder pw = new StringBuilder();
            for (int i = 0; i < numSyllables; i++)
            {
                pw.Append(MakeSyllable());

                if (numNumeric > 0 && ((rand.Next() % 2) == 0))
                {
                    pw.Append(MakeNumeric());
                    numNumeric--;
                }

                if (numSymbols > 0 && ((rand.Next() % 2) == 0))
                {
                    pw.Append(MakeSymbol());
                    numSymbols--;
                }
            }

            while (numNumeric > 0)
            {
                pw.Append(MakeNumeric());
                numNumeric--;
            }

            while (numSymbols > 0)
            {
                pw.Append(MakeSymbol());
                numSymbols--;
            }

            return pw.ToString();
        }

        private static char MakeSymbol()
        {
            return SYMBOLS[rand.Next(SYMBOLS.Length)];

        }

        private static char MakeNumeric()
        {
            return NUMBERS[rand.Next(NUMBERS.Length)];
        }

        private static string MakeSyllable()
        {
            int len = rand.Next(3, 5); // will return either 3 or 4

            StringBuilder syl = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                char c;
                if (i == 1) // the second should be a vowel, all else a consonant
                    c = VOWELS[rand.Next(VOWELS.Length)];
                else
                    c = CONSONANTS[rand.Next(CONSONANTS.Length)];

                // only first character can be uppercase
                if (i == 0 && (rand.Next() % 2) == 0)
                    c = Char.ToUpper(c);

                // append
                syl.Append(c);
            }

            return syl.ToString();
        }
    }
}
