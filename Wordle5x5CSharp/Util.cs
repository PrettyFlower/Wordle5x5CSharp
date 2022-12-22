using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public static class Util
    {
        public const string FREQUENCY_ALPHABET = "qxjzvfwbkgpmhdcytlnuroisea";
        public static int[] FrequencyAlphabet = new int[26];
        public static List<Word>[] Words = new List<Word>[26];

        public static void Load()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < FREQUENCY_ALPHABET.Length; i++)
            {
                FrequencyAlphabet[i] = 1 << (FREQUENCY_ALPHABET[i] - 97);
            }
            for (int i = 0; i < Words.Length; i++)
            {
                Words[i] = new List<Word>(1024);
            }
            var wordHashes = new HashSet<int>(6000);
            using (var sr = new StreamReader(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != 5)
                        continue;
                    var word = StrToBits(line);
                    if (word.bits == 0)
                        continue;
                    if (wordHashes.Contains(word.bits))
                        continue;
                    wordHashes.Add(word.bits);
                    Words[word.bestLetter].Add(word);
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

        public static Word StrToBits(string s)
        {
            var word = new Word();
            int bits = 0;
            int bestLetter = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var bitOffset = s[i] - 97;
                var bit = 1 << bitOffset;
                if ((bits & bit) > 0)
                    return word;
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
            word.text = s;
            word.bits = bits;
            word.bestLetter = bestLetter;
            return word;
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
    }
}
