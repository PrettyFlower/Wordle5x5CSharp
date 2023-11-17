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
        [StructLayout(LayoutKind.Sequential, Size = 8)]
        public struct WordInfo
        {
            public int Idx;
            public int Bits;

            public override string ToString()
            {
                return WordIdxToText(Idx);
            }
        }

        public static string INPUT_FILE;
        public static string OUTPUT_FILE;
        public static int LINE_LENGTH;
        public const string FREQUENCY_ALPHABET = "qxjzvfwbkgpmhdcytlnuroisea";
        public const int SUBMASK_BUCKETS = 6;
        public static int[] FrequencyAlphabet = new int[26];
        public static List<string> WordIdxsToText = new List<string>();
        public static List<WordInfo>[][] LetterIndex = new List<WordInfo>[26][];

        static Util() {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                INPUT_FILE = @"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt";
                OUTPUT_FILE = @"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt";
            }
            else
            {
                INPUT_FILE = @"/home/prettyflower/code/Wordle5x5CSharp/Wordle5x5CSharp/words_alpha.txt";
                OUTPUT_FILE = @"/home/prettyflower/code/Wordle5x5CSharp/Wordle5x5CSharp/results.txt";
            }
        }

        public static void Load()
        {
            var sw = Stopwatch.StartNew();
            WordIdxsToText.Clear();
            for (int i = 0; i < FREQUENCY_ALPHABET.Length; i++)
            {
                FrequencyAlphabet[i] = 1 << (FREQUENCY_ALPHABET[i] - 97);
            }
            for (int i = 0; i < LetterIndex.Length; i++)
            {
                LetterIndex[i] = new List<WordInfo>[SUBMASK_BUCKETS];
                for(int j = 0; j < SUBMASK_BUCKETS; j++)
                {
                    LetterIndex[i][j] = new List<WordInfo>(500);
                }
            }
            var wordHashes = new HashSet<int>(6000);
            sw.Stop();
            Console.WriteLine($"Setup: {sw.ElapsedMilliseconds}");

            // it is a bit faster to read the whole file in as an array of bytes and parse it manually
            // so that's why we're doing this crazyness
            sw.Restart();
            var bytes = File.ReadAllBytes(INPUT_FILE);
            sw.Stop();
            Console.WriteLine($"Read file: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var buffer = new char[5];
            int fileIdx = 0;
            char c = (char)bytes[fileIdx];

            // figure out what our line endings are (issue #1)
            LINE_LENGTH = 6;
            do
            {
                c = (char)bytes[fileIdx];
                if (c == '\r')
                    LINE_LENGTH++;
                fileIdx++;
            } while (c != '\n');
            fileIdx = 0;

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
                if (lineIdx != LINE_LENGTH)
                    continue;

                var line = string.Create(5, buffer, (span, b) => buffer.CopyTo(span));
                var isValid = StrToBits(line, out var bits, out var bestLetter);
                if (!isValid)
                    continue;
                if (wordHashes.Contains(bits))
                    continue;
                wordHashes.Add(bits);
                for(int bucket = 0; bucket < SUBMASK_BUCKETS; bucket++)
                {
                    var submask = GetSubmask(bucket);
                    if (bucket == SUBMASK_BUCKETS - 1 || (bits & submask) > 0)
                    {
                        var wordIdx = WordIdxsToText.Count;
                        WordIdxsToText.Add(line);
                        var wordInfo = new WordInfo { Idx = wordIdx, Bits = bits };
                        LetterIndex[bestLetter][bucket].Add(wordInfo);
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

        public static bool StartsWith(int[] words, int numWords, string list)
        {
            var split = list.Split(' ');
            if (numWords < split.Length)
                return false;
            for (int i = 0; i < split.Length; i++)
            {
                var wordText = Util.WordIdxToText(words[i]);
                if (wordText != split[i])
                    return false;
            }
            return true;
        }

        public static int GetSubmask(int i)
        {
            return GetLetterBit(FREQUENCY_ALPHABET.Length - i - 1);
        }

        public static string WordIdxToText(int wordIdx)
        {
            if (wordIdx < 0)
                return "null";
            return WordIdxsToText[wordIdx];
        }
    }
}
