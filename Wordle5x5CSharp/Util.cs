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
        public const string FREQUENCY_ALPHABET = "qxjzvfwbkgpmhdcytlnuroisea";
        public const int SUBMASK_BUCKETS = 6;
        public static int[] FrequencyAlphabet = new int[26];
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

            // it is a bit faster to read the whole file in as an array of bytes and parse it manually
            // so that's why we're doing this crazyness
            sw.Restart();
            var bytes = File.ReadAllBytes(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt");
            sw.Stop();
            Console.WriteLine($"Read file: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var buffer = new char[5];
            int fileIdx = 0;
            char c = (char)bytes[fileIdx];
            while (fileIdx < bytes.Length)
            {
                var lineIdx = 0;
                do
                {
                    c = (char)bytes[fileIdx + lineIdx];
                    if (lineIdx < buffer.Length)
                        buffer[lineIdx] = c;
                    lineIdx++;
                } while (c != '\n');
                fileIdx += lineIdx;
                if (lineIdx != 7)
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
