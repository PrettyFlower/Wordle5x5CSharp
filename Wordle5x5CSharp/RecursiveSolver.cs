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
            Solve(0, new string[5], 0, 0, 0);
            sw.Stop();
            Console.WriteLine($"Solve: {sw.ElapsedMilliseconds}");

            sw.Restart();
            using (var writer = new StreamWriter(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                foreach (var solution in Solutions)
                {
                    writer.WriteLine(string.Join(" ", solution.Select(w => w)));
                }
            }
            sw.Stop();
            Console.WriteLine($"Final write: {sw.ElapsedMilliseconds}");
        }

        public static void Solve(int bits, string?[] wordsSoFar, int letterIdx, int numWords, int numSkips)
        {
            if (numSkips == 2)
                return;

            if (letterIdx == Util.Words.Length)
                return;

            if(numWords == 5)
            {
                var solutionArr = new string[5];
                Array.Copy(wordsSoFar, solutionArr, 5);
                Solutions.Add(solutionArr);
                return;
            }

            if ((bits & Util.GetLetterBit(letterIdx)) == 0)
            {
                foreach (var word in Util.Words[letterIdx])
                {
                    if ((word.bits & bits) > 0)
                        continue;
                    wordsSoFar[numWords] = word.text;
                    var newBits = bits | word.bits;
                    Solve(newBits, wordsSoFar, letterIdx + 1, numWords + 1, numSkips);
                    wordsSoFar[numWords] = null;
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
