using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public class WordList
    {
        public int bits;
        public string[] words;
        public int numWords;
        public int skips;

        public bool StartsWith(string list)
        {
            return Util.StartsWith(words, numWords, list);
        }

        public override string ToString()
        {
            var wordsStr = string.Join(", ", words.Select(w => w ?? "null"));
            return $"{wordsStr} {Util.ToBinary(bits)}";
        }
    }

    public static class WordListSolver
    {
        public static void Solve()
        {
            var sw = Stopwatch.StartNew();
            var wordLists1 = AddFirstWordLists(0);
            sw.Stop();
            Console.WriteLine($"FirstWordLists: {sw.ElapsedMilliseconds}");

            sw.Restart();
            wordLists1 = AddSecondWordLists(wordLists1, 1);
            sw.Stop();
            Console.WriteLine($"SecondWordLists: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var wordLists2 = AddFirstWordLists(2);
            sw.Stop();
            Console.WriteLine($"FirstWordLists2: {sw.ElapsedMilliseconds}");

            sw.Restart();
            wordLists2 = AddSecondWordLists(wordLists2, 3);
            sw.Stop();
            Console.WriteLine($"SecondWordLists2: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var wordLists = CombineWordLists(wordLists1, wordLists2);
            sw.Stop();
            Console.WriteLine($"CombineWordLists ({(wordLists1.Count * wordLists2.Count):n0}): {sw.ElapsedMilliseconds}");

            for (int i = 4; i < Util.FREQUENCY_ALPHABET.Length; i++)
            {
                sw.Restart();
                wordLists = AddRemainingWordLists(wordLists, i);
                sw.Stop();
                Console.WriteLine($"RemainingWordLists {i} ({(wordLists.Count * Util.Words[i].Count):n0}): {sw.ElapsedMilliseconds}");
            }

            sw.Restart();
            using (var writer = new StreamWriter(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                foreach (var wordList in wordLists)
                {
                    writer.WriteLine(string.Join(" ", wordList.words));
                }
            }
            sw.Stop();
            Console.WriteLine($"Final write: {sw.ElapsedMilliseconds}");
        }

        static List<WordList> AddFirstWordLists(int letterIdx)
        {
            var wordLists = new List<WordList>();
            foreach (var word in Util.Words[letterIdx])
            {
                var wordList = new WordList
                {
                    bits = word.bits,
                    words = new string[5] { word.text, null, null, null, null },
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
            foreach (var wordList in wordLists)
            {
                var newWordList = new WordList
                {
                    bits = wordList.bits,
                    words = new string[5] { wordList.words[0], null, null, null, null },
                    numWords = 1,
                    skips = (wordList.bits & Util.GetLetterBit(letterIdx)) > 0 ? 0 : 1
                };
                newWordLists.Add(newWordList);
            }
            foreach (var word in Util.Words[letterIdx])
            {
                foreach (var wordList in wordLists)
                {
                    if ((wordList.bits & word.bits) > 0)
                        continue;
                    var doubleWordList = new WordList
                    {
                        bits = wordList.bits | word.bits,
                        words = new string[5] { wordList.words[0], word.text, null, null, null },
                        numWords = 2,
                        skips = 0
                    };
                    newWordLists.Add(doubleWordList);
                }
                var newWordList = new WordList
                {
                    bits = word.bits,
                    words = new string[5] { word.text, null, null, null, null },
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
            var aArr = a.ToArray();
            var bArr = b.ToArray();
            foreach (var wordListA in aArr)
            {
                foreach (var wordListB in bArr)
                {
                    if ((wordListA.bits & wordListB.bits) > 0)
                        continue;
                    var allBits = wordListA.bits | wordListB.bits;
                    var numLetters = 0;
                    numLetters += (allBits & Util.GetLetterBit(0)) >> (Util.FREQUENCY_ALPHABET[0] - 97);
                    numLetters += (allBits & Util.GetLetterBit(1)) >> (Util.FREQUENCY_ALPHABET[1] - 97);
                    numLetters += (allBits & Util.GetLetterBit(2)) >> (Util.FREQUENCY_ALPHABET[2] - 97);
                    numLetters += (allBits & Util.GetLetterBit(3)) >> (Util.FREQUENCY_ALPHABET[3] - 97);
                    if (numLetters < 3)
                        continue;
                    var newWords = new string[5];
                    Array.Copy(wordListA.words, newWords, wordListA.numWords);
                    Array.Copy(wordListB.words, 0, newWords, wordListA.numWords, wordListB.numWords);
                    var newWordList = new WordList
                    {
                        bits = allBits,
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
            var letterBit = Util.GetLetterBit(letterIdx);
            var wordsArr = Util.Words[letterIdx].ToArray();
            foreach (var wordList in wordLists)
            {
                if ((wordList.bits & letterBit) > 0)
                {
                    newWordLists.Add(wordList);
                    continue;
                }
                foreach (var word in wordsArr)
                {
                    if ((wordList.bits & word.bits) > 0)
                        continue;
                    var newWords = new string[5];
                    Array.Copy(wordList.words, newWords, wordList.numWords);
                    newWords[wordList.numWords] = word.text;
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
    }
}
