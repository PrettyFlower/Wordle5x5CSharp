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
        public static List<int[]> Solutions = new List<int[]>();

        public static void Solve()
        {
            var sw = Stopwatch.StartNew();
            Solve(0, new int[5], 0, 0, 0);
            sw.Stop();
            Console.WriteLine($"Solve: {sw.ElapsedMilliseconds}");

            sw.Restart();
            using (var writer = new StreamWriter(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt", false))
            {
                foreach (var solution in Solutions)
                {
                    foreach(var wordIdx in solution)
                    {
                        var wordText = Util.WordIdxsToText[wordIdx];
                        writer.Write(wordText + " ");
                    }
                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine($"Final write: {sw.ElapsedMilliseconds}");
            Solutions.Clear();
        }

        public static void Solve(int bits, int[] wordsSoFar, int letterIdx, int numWords, int numSkips)
        {
            if (numSkips == 2)
                return;

            if (letterIdx == Util.LetterIndex.Length)
                return;

            if(numWords == 5)
            {
                var solutionArr = new int[5];
                Array.Copy(wordsSoFar, solutionArr, solutionArr.Length);
                Solutions.Add(solutionArr);
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
