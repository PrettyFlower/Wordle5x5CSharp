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

            var totalSw = Stopwatch.StartNew();
            Util.Load();
            //WordListSolver.Solve();
            RecursiveSolver.Solve();
            totalSw.Stop();
            Console.WriteLine($"Total time: {totalSw.ElapsedMilliseconds}");
        }
    }
}