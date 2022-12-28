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

            var iterations = 1;
            if (args.Length > 0)
                iterations = int.Parse(args[0]);
            long min = long.MaxValue, max = 0, total = 0;
            for (int i = 0; i < iterations; i++)
            {
                var totalSw = Stopwatch.StartNew();
                Util.Load();
                //WordListSolver.Solve();
                RecursiveSolver.Solve();
                totalSw.Stop();
                Console.WriteLine($"Total time: {totalSw.ElapsedMilliseconds}");
                Console.WriteLine();
                total += totalSw.ElapsedMilliseconds;
                if (totalSw.ElapsedMilliseconds < min)
                    min = totalSw.ElapsedMilliseconds;
                if (totalSw.ElapsedMilliseconds > max)
                    max = totalSw.ElapsedMilliseconds;
            }
            Console.WriteLine($"Average: {total / iterations}, min: {min}, max: {max}");
        }
    }
}