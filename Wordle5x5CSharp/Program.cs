using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static Wordle5x5CSharp.Program;

namespace Wordle5x5CSharp
{
    public class Program
    {
        public class Word
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
                return $"{text} {ToBinary(bits)} {bestLetter}";
            }
        }

        public class WordList
        {
            public int bits;
            public Word[] words;
            public int numWords;
            public int skips;

            public bool StartsWith(string list)
            {
                var split = list.Split(' ');
                if (numWords < split.Length)
                    return false;
                for (int i = 0; i < split.Length; i++)
                {
                    if (words[i].text != split[i])
                        return false;
                }
                return true;
            }

            public override string ToString()
            {
                var wordsStr = string.Join(", ", words.Select(w => w?.text ?? "null"));
                return $"{wordsStr} {ToBinary(bits)}";
            }
        }

        public const string FREQUENCY_ALPHABET = "qxjzvfwbkgpmhdcytlnuroisea";
        public static int[] FrequencyAlphabet = new int[26];
        public static HashSet<Word>[] Words = new HashSet<Word>[26];

        static void Main(string[] args)
        {
            Diff();
            //CalculateSolutions();
        }

        static void CalculateSolutions()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < FREQUENCY_ALPHABET.Length; i++)
            {
                FrequencyAlphabet[i] = GetLetterBit(i);
            }
            for (int i = 0; i < Words.Length; i++)
            {
                Words[i] = new HashSet<Word>(1024);
            }
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
                    Words[word.bestLetter].Add(word);
                }
            }
            var wordLists1 = AddFirstWordLists(0);
            wordLists1 = AddSecondWordLists(wordLists1, 1);
            var wordLists2 = AddFirstWordLists(2);
            wordLists2 = AddSecondWordLists(wordLists2, 3);
            var wordLists = CombineWordLists(wordLists1, wordLists2);
            for (int i = 4; i < FREQUENCY_ALPHABET.Length; i++)
            {
                wordLists = AddRemainingWordLists(wordLists, i);
            }
            using (var writer = new StreamWriter(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                foreach (var wordList in wordLists)
                {
                    writer.WriteLine(string.Join(" ", wordList.words.Select(w => w.text)));
                }
            }
            sw.Stop();
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
        }

        static Word StrToBits(string s)
        {
            var word = new Word();
            int bits = 0;
            int bestLetter = 0;
            for(int i = 0; i < s.Length; i++)
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

        static List<WordList> AddFirstWordLists(int letterIdx)
        {
            var wordLists = new List<WordList>();
            foreach (var word in Words[letterIdx])
            {
                var wordList = new WordList
                {
                    bits = word.bits,
                    words = new Word[5] { word, null, null, null, null },
                    numWords = 1,
                    skips = 0
                };
                wordLists.Add(wordList);
            }
            return wordLists;
        }

        static List<WordList> AddSecondWordLists(List<WordList> wordLists, int letterIdx)
        {
            var newWordLists = new List<WordList>();
            foreach(var wordList in wordLists)
            {
                var newWordList = new WordList
                {
                    bits = wordList.bits,
                    words = new Word[5] { wordList.words[0], null, null, null, null },
                    numWords = 1,
                    skips = (wordList.bits & GetLetterBit(letterIdx)) > 0 ? 0 : 1
                };
                newWordLists.Add(newWordList);
            }
            foreach (var word in Words[letterIdx])
            {
                foreach (var wordList in wordLists)
                {
                    if ((wordList.bits & word.bits) > 0)
                        continue;
                    var doubleWordList = new WordList
                    {
                        bits = wordList.bits | word.bits,
                        words = new Word[5] { wordList.words[0], word, null, null, null },
                        numWords = 2,
                        skips = 0
                    };
                    newWordLists.Add(doubleWordList);
                }
                var newWordList = new WordList
                {
                    bits = word.bits,
                    words = new Word[5] { word, null, null, null, null },
                    numWords = 1,
                    skips = 1
                };
                newWordLists.Add(newWordList);
            }
            return newWordLists;
        }

        static List<WordList> CombineWordLists(List<WordList> a, List<WordList> b)
        {
            var newWordLists = new List<WordList>();
            foreach(var wordListA in a)
            {
                foreach(var wordListB in b)
                {
                    if ((wordListA.bits & wordListB.bits) > 0)
                        continue;
                    var allBits = wordListA.bits | wordListB.bits;
                    var numLetters = 0;
                    numLetters += (allBits & GetLetterBit(0)) >> (FREQUENCY_ALPHABET[0] - 97);
                    numLetters += (allBits & GetLetterBit(1)) >> (FREQUENCY_ALPHABET[1] - 97);
                    numLetters += (allBits & GetLetterBit(2)) >> (FREQUENCY_ALPHABET[2] - 97);
                    numLetters += (allBits & GetLetterBit(3)) >> (FREQUENCY_ALPHABET[3] - 97);
                    if (numLetters < 3)
                        continue;
                    var newWords = new Word[5];
                    Array.Copy(wordListA.words, newWords, wordListA.numWords);
                    Array.Copy(wordListB.words, 0, newWords, wordListA.numWords, wordListB.numWords);
                    var newWordList = new WordList
                    {
                        bits = wordListA.bits | wordListB.bits,
                        words = newWords,
                        numWords = wordListA.numWords + wordListB.numWords,
                        skips = 4 - numLetters
                    };
                    newWordLists.Add(newWordList);
                }
            }
            return newWordLists;
        }

        static List<WordList> AddRemainingWordLists(List<WordList> wordLists, int letterIdx)
        {
            var newWordLists = new List<WordList>();
            var letterBit = GetLetterBit(letterIdx);
            foreach (var wordList in wordLists)
            {
                if ((wordList.bits & letterBit) > 0)
                {
                    newWordLists.Add(wordList);
                    continue;
                }
                foreach (var word in Words[letterIdx])
                {
                    if ((wordList.bits & word.bits) > 0)
                        continue;
                    var newWords = new Word[5];
                    Array.Copy(wordList.words, newWords, wordList.numWords);
                    newWords[wordList.numWords] = word;
                    var newWordList = new WordList
                    {
                        bits = wordList.bits | word.bits,
                        words = newWords,
                        numWords = wordList.numWords + 1
                    };
                    newWordLists.Add(newWordList);
                }
                if (wordList.skips == 0)
                {
                    wordList.skips++;
                    newWordLists.Add(wordList);
                }
            }
            return newWordLists;
        }

        static void Diff()
        {
            var expected = new HashSet<string>();
            using (var sr = new StreamReader(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results_no_anagrams.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Trim();
                    expected.Add(line);
                }
            }

            var results = new HashSet<string>();
            using(var sr = new StreamReader(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    results.Add(line);
                }
            }

            if (expected.Count == results.Count)
                Console.WriteLine($"Counts match at {expected.Count}");
            else
                Console.WriteLine($"Expected has {expected.Count} but results have {results.Count}");

            var mismatchDetected = false;
            int i = 0;
            foreach(var line in expected)
            {
                if (!ContainsEquivalent(results, line))
                {
                    Console.WriteLine($"{i.ToString().PadLeft(4, '0')} Expected {line} but was not found in results");
                    mismatchDetected = true;
                }
                i++;
            }

            i = 0;
            foreach (var line in results)
            {
                if (!ContainsEquivalent(expected, line))
                {
                    Console.WriteLine($"{i.ToString().PadLeft(4, '0')} Results has line {line} but was not in expected");
                    mismatchDetected = true;
                }
                i++;
            }
            if (!mismatchDetected)
                Console.WriteLine("Results match!");
        }

        static bool ContainsEquivalent(HashSet<string> hs, string s)
        {
            foreach(var line in hs)
            {
                if (IsEquivalent(line, s))
                    return true;
            }
            return false;
        }

        static bool IsEquivalent(string a, string b)
        {
            var splitA = a.Split(' ');
            var splitB = b.Split(' ');
            for(int i = 0; i < splitA.Length; i++)
            {
                if (!splitB.Contains(splitA[i]))
                    return false;
            }
            return true;
        }

        static int GetLetterBit(int letterIdx)
        {
            return 1 << (FREQUENCY_ALPHABET[letterIdx] - 97);
        }

        static string ToBinary(int bits)
        {
            return Convert.ToString(bits, 2).PadLeft(26, '0');
        }
    }
}