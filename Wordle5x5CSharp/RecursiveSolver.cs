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
        public static List<string[]> Solutions = new List<string[]>();

        public static void Solve()
        {
            var sw = Stopwatch.StartNew();
            Solve(0, new Tuple<int, int, int>[5], 0, 0, 0);
            sw.Stop();
            Console.WriteLine($"Solve: {sw.ElapsedMilliseconds}");

            sw.Restart();
            using (var writer = new StreamWriter(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                foreach (var solution in Solutions)
                {
                    writer.WriteLine(string.Join(" ", solution));
                }
            }
            sw.Stop();
            Console.WriteLine($"Final write: {sw.ElapsedMilliseconds}");
        }

        public static void Solve(int bits, Tuple<int, int, int>[] wordsSoFar, int letterIdx, int numWords, int numSkips)
        {
            if (numSkips == 2)
                return;

            if (letterIdx == Util.WordText.Length)
                return;

            if(numWords == 5)
            {
                var solutionArr = new string[5];
                for(int i = 0; i < 5; i++)
                {
                    var tuple = wordsSoFar[i];
                    solutionArr[i] = Util.WordText[tuple.Item1][tuple.Item2][tuple.Item3];
                }
                Solutions.Add(solutionArr);
                return;
            }

            if ((bits & Util.GetLetterBit(letterIdx)) == 0)
            {
                for(int i = 0; i < 4; i++)
                {
                    var submask = Util.GetSubmask(i);
                    if (i == 3 || (bits & submask) == 0)
                    {
                        var wordList = Util.WordBits[letterIdx][i];
                        for (int j = 0; j < wordList.Count; j++)
                        {
                            var wordBits = wordList[j];
                            if ((wordBits & bits) > 0)
                                continue;
                            wordsSoFar[numWords] = Tuple.Create(letterIdx, i, j);
                            var newBits = bits | wordBits;
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
