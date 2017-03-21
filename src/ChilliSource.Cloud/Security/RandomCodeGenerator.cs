using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ChilliSource.Cloud.DataStructures;

namespace ChilliSource.Cloud.Security
{
    public class RandomCodeGenerator
    {
        private static IThreadSafeRandom _random = ThreadSafeRandom.Get();

        public static string GenerateCode(int length, params ICharacterSet[] characterSets)
        {
            var characters = CharacterSet.CombineSets(characterSets);
            return GenerateStringCode(length, characters);
        }

        public static string GenerateCode(int length, char[] allowedCharacters)
        {
            return GenerateStringCode(length, CharacterSet.FromChars(allowedCharacters));
        }

        private static string GenerateStringCode(int length, ICharacterSet characterSet)
        {
            var result = new char[length];
            var i = 0;
            foreach (var c in GenerateCodeInternal(length, characterSet))
            {
                result[i++] = c;
            }

            return new string(result);
        }

        private static IEnumerable<char> GenerateCodeInternal(int length, ICharacterSet characterSet)
        {
            var setCount = characterSet.Count;
            if (setCount < 2)
                throw new ApplicationException("The set must have at least two characters");

            for (int i = 0; i < length; i++)
                yield return characterSet[_random.Next(0, setCount)];
        }
    }
}
