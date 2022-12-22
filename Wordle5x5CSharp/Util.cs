using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public static class Util
    {
        /*public class Word
        {
            public string text;
            public int bits;
            public int bestLetter;

            public override int GetHashCode()
            {
                return bits;
            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                return bits == ((Word)obj).bits;
            }

            public override string ToString()
            {
                return $"{text} {Util.ToBinary(bits)} {bestLetter}";
            }
        }*/

        public const string FREQUENCY_ALPHABET = "qxjzvfwbkgpmhdcytlnuroisea";
        public static int[] FrequencyAlphabet = new int[26];
        //public static List<Word>[][] Words = new List<Word>[26][];
        public static List<string>[][] WordText = new List<string>[26][];
        public static List<int>[][] WordBits = new List<int>[26][];

        public static void Load()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < FREQUENCY_ALPHABET.Length; i++)
            {
                FrequencyAlphabet[i] = 1 << (FREQUENCY_ALPHABET[i] - 97);
            }
            for (int i = 0; i < WordText.Length; i++)
            {
                WordText[i] = new List<string>[4];
                WordBits[i] = new List<int>[4];
                for(int j = 0; j < 4; j++)
                {
                    WordText[i][j] = new List<string>(500);
                    WordBits[i][j] = new List<int>(500);
                }
            }
            var wordHashes = new HashSet<int>(6000);
            using (var sr = new StreamReader(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != 5)
                        continue;
                    var isValid = StrToBits(line, out var bits, out var bestLetter);
                    if (!isValid)
                        continue;
                    if (wordHashes.Contains(bits))
                        continue;
                    wordHashes.Add(bits);
                    for(int i = 0; i < 4; i++)
                    {
                        var submask = GetSubmask(i);
                        if (i == 3 || (bits & submask) > 0)
                        {
                            WordText[bestLetter][i].Add(line);
                            WordBits[bestLetter][i].Add(bits);
                            break;
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine($"Setup: {sw.ElapsedMilliseconds}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLetterBit(int letterIdx)
        {
            return FrequencyAlphabet[letterIdx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBinary(int bits)
        {
            return Convert.ToString(bits, 2).PadLeft(26, '0');
        }

        public static bool StrToBits(string s, out int bits, out int bestLetter)
        {
            bits = 0;
            bestLetter = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var bitOffset = s[i] - 97;
                var bit = 1 << bitOffset;
                if ((bits & bit) > 0)
                    return false;
                bits |= bit;
            }
            for (int i = 0; i < FrequencyAlphabet.Length; i++)
            {
                if ((bits & FrequencyAlphabet[i]) > 0)
                {
                    bestLetter = i;
                    break;
                }
            }
            return true;
        }

        public static bool StartsWith(string[] words, int numWords, string list)
        {
            var split = list.Split(' ');
            if (numWords < split.Length)
                return false;
            for (int i = 0; i < split.Length; i++)
            {
                if (words[i] != split[i])
                    return false;
            }
            return true;
        }

        public static int GetSubmask(int i)
        {
            return GetLetterBit(FREQUENCY_ALPHABET.Length - i - 1);
        }
    }
}
