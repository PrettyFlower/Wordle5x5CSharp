using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public static class RecursiveSolver
    {
        public static int[][] Solutions = new int[10000][];
        public static int SolutionCount = -1;

        public static void Solve()
        {
            SolutionCount = -1;
            for(int i = 0; i < 1000; i++)
            {
                Solutions[i] = new int[5];
            }

            var sw = Stopwatch.StartNew();
            //var firstIndexWords = Util.LetterIndex[0].Sum(sl => sl.Count);
            //var numParallelWords = firstIndexWords + Util.LetterIndex[1].Sum(sl => sl.Count);
            var firstWords = Util.LetterIndex[0].SelectMany(sl => sl).ToList();
            var secondWords = Util.LetterIndex[1].SelectMany(sl => sl).ToList();
            var startingWords = firstWords.Concat(secondWords).ToList();
            sw.Stop();
            Console.WriteLine($"Generate starting words: {sw.ElapsedMilliseconds}");

            sw.Restart();
            Parallel.ForEach(startingWords, (wordInfo, state, idx) =>
            {
                var isFirstWord = idx < firstWords.Count;
                var letterIdx = isFirstWord ? 1 : 2;
                var numSkips = isFirstWord ? 0 : 1;
                var arr = new int[5] { wordInfo.Idx, 0, 0, 0, 0 };
                Solve(wordInfo.Bits, arr, letterIdx, 1, numSkips);
            });
            sw.Stop();
            Console.WriteLine($"Solve: {sw.ElapsedMilliseconds}");

            sw.Restart();
            using (var writer = new StreamWriter(Util.OUTPUT_FILE, false))
            {
                for(int i = 0; i <= SolutionCount; i++)
                {
                    var solution = Solutions[i];
                    foreach(var wordIdx in solution)
                    {
                        var wordText = Util.WordIdxsToText[wordIdx];
                        writer.Write(wordText + " ");
                    }
                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine($"Final write: {sw.ElapsedMilliseconds}, num solutions: {SolutionCount}");
        }

        public static void Solve(int bits, int[] wordsSoFar, int letterIdx, int numWords, int numSkips)
        {
            if (numSkips == 2)
                return;

            if (letterIdx == Util.LetterIndex.Length)
                return;

            if(numWords == 5)
            {
                int solutionCount = Interlocked.Increment(ref SolutionCount);
                var solutionArr = new int[5];
                Array.Copy(wordsSoFar, solutionArr, solutionArr.Length);
                Solutions[solutionCount] = solutionArr;
                return;
            }

            if ((bits & Util.GetLetterBit(letterIdx)) == 0)
            {
                for(int bucket = 0; bucket < Util.SUBMASK_BUCKETS; bucket++)
                {
                    var submask = Util.GetSubmask(bucket);
                    if (bucket == Util.SUBMASK_BUCKETS - 1 || (bits & submask) == 0)
                    {
                        var wordList = Util.LetterIndex[letterIdx][bucket];
                        for (int j = 0; j < wordList.Count; j++)
                        {
                            var wordInfo = wordList[j];
                            if ((wordInfo.Bits & bits) > 0)
                                continue;
                            wordsSoFar[numWords] = wordInfo.Idx;
                            var newBits = bits | wordInfo.Bits;
                            Solve(newBits, wordsSoFar, letterIdx + 1, numWords + 1, numSkips);
                        }
                    }
                }
                Solve(bits, wordsSoFar, letterIdx + 1, numWords, numSkips + 1);
            }
            else
            {
                Solve(bits, wordsSoFar, letterIdx + 1, numWords, numSkips);
            }
        }
    }
}
