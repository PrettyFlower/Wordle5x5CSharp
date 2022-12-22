using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Wordle5x5CSharp
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Diff.Check();
            //return;

            long min = long.MaxValue, max = 0, total = 0;
            for (int i = 0; i < 10; i++)
            {
                var totalSw = Stopwatch.StartNew();
                Util.Load();
                //WordListSolver.Solve();
                RecursiveSolver.Solve();
                totalSw.Stop();
                Console.WriteLine($"Total time: {totalSw.ElapsedMilliseconds}");
                total += totalSw.ElapsedMilliseconds;
                if (totalSw.ElapsedMilliseconds < min)
                    min = totalSw.ElapsedMilliseconds;
                if (totalSw.ElapsedMilliseconds > max)
                    max = totalSw.ElapsedMilliseconds;
            }
            Console.WriteLine($"Average: {total / 10}, min: {min}, max: {max}");
        }
    }
}