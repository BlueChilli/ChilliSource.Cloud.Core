using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public interface ICharacterSet : IReadOnlyList<char>
    {
    }

    public static class CharacterSet
    {
        public readonly static ICharacterSet Numbers = CharacterSet.FromRange('0', '9');
        public readonly static ICharacterSet LowerCaseVowels = CharacterSet.FromChars('a', 'e', 'i', 'o', 'u');
        public readonly static ICharacterSet LowerCaseLetters = CharacterSet.FromRange('a', 'z');

        public static ICharacterSet ToUpper(this ICharacterSet set)
        {
            var result = new char[set.Count];
            var i = 0;
            foreach (var c in set)
            {
                result[i++] = Char.ToUpperInvariant(c);
            }

            return FromChars(result);
        }

        public static ICharacterSet FromChars(params char[] chars)
        {
            return new CharacterSetImplementation(chars);
        }

        public static ICharacterSet FromRange(char lowChar, char highChar)
        {
            return new CharacterRange(lowChar, highChar);
        }

        public static ICharacterSet CombineSets(params ICharacterSet[] sets)
        {
            if (sets == null)
                throw new ArgumentNullException("CompositeCharacterSet: sets");

            if (sets.Length == 1)
                return sets[0];

            return new CompositeCharacterSet(sets);
        }

        public static bool Validate(this ICharacterSet set, string s)
        {
            if (String.IsNullOrEmpty(s)) return true;
            for(var i = 0; i < s.Length; i++)
            {
                if (!set.Contains(s[0])) return false;
            }
            return true;
        }

        private class CharacterSetImplementation : ICharacterSet
        {
            char[] _chars;
            public CharacterSetImplementation(params char[] chars)
            {
                if (chars == null)
                    throw new ArgumentNullException("CharacterSet: chars");
                _chars = chars;
            }

            public char this[int index] { get { return _chars[index]; } }

            public int Count { get { return _chars.Length; } }

            public IEnumerator<char> GetEnumerator()
            {
                return ((IEnumerable<char>)_chars).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _chars.GetEnumerator();
            }
        }

        private class CharacterRange : CharacterSetImplementation
        {
            public CharacterRange(char lowChar, char highChar)
                : base(CharacterRange.GetCharArray(lowChar, highChar).ToArray())
            {
            }

            private static IEnumerable<char> GetCharArray(char lowChar, char highChar)
            {
                if (highChar < lowChar)
                {
                    var temp = lowChar;
                    lowChar = highChar;
                    highChar = temp;
                }
                for (var d = lowChar; d <= highChar; d++)
                    yield return d;
            }
        }

        private class CompositeCharacterSet : CharacterSetImplementation
        {
            public CompositeCharacterSet(ICharacterSet[] sets)
                : base(GetCompositeArray(sets))
            {
            }

            private static char[] GetCompositeArray(ICharacterSet[] sets)
            {
                return sets.SelectMany(set => set).Distinct().ToArray();
            }
        }
    }
}
