using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        public const int SUBMASK_BUCKETS = 6;
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
                WordText[i] = new List<string>[SUBMASK_BUCKETS];
                WordBits[i] = new List<int>[SUBMASK_BUCKETS];
                for(int j = 0; j < SUBMASK_BUCKETS; j++)
                {
                    WordText[i][j] = new List<string>(500);
                    WordBits[i][j] = new List<int>(500);
                }
            }
            var wordHashes = new HashSet<int>(6000);
            sw.Stop();
            Console.WriteLine($"Setup: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var bytes = new byte[5_000_000];
            var ms = new MemoryStream(bytes);
            using(var fs = File.OpenRead(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt"))
            {
                fs.Read(bytes, 0, bytes.Length);
            }
            var fileText = Encoding.UTF8.GetString(bytes).ToCharArray();
            sw.Stop();
            Console.WriteLine($"Read file: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var buffer = new char[5];
            char c;
            for (int i = 0; i < fileText.Length; i++)
            {
                c = fileText[i];
                if (c == '\0')
                    break;
                buffer[0] = c;
                var j = 1;
                while (c != '\n')
                {
                    c = fileText[i + j];
                    if (j < buffer.Length)
                        buffer[j] = c;
                    j++;
                }
                i += j - 1;
                if (j != 7)
                    continue;

                var line = string.Create(5, buffer, (span, b) => buffer.CopyTo(span));
                var isValid = StrToBits(line, out var bits, out var bestLetter);
                if (!isValid)
                    continue;
                if (wordHashes.Contains(bits))
                    continue;
                wordHashes.Add(bits);
                for(int k = 0; k < SUBMASK_BUCKETS; k++)
                {
                    var submask = GetSubmask(k);
                    if (k == SUBMASK_BUCKETS - 1 || (bits & submask) > 0)
                    {
                        WordText[bestLetter][k].Add(line);
                        WordBits[bestLetter][k].Add(bits);
                        break;
                    }
                }
            }
            sw.Stop();
            Console.WriteLine($"Parse file: {sw.ElapsedMilliseconds}");
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
