using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public static class Diff
    {
        public static void Check()
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
            using (var sr = new StreamReader(@"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\results.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Trim();
                    results.Add(line);
                }
            }

            if (expected.Count == results.Count)
                Console.WriteLine($"Counts match at {expected.Count}");
            else
                Console.WriteLine($"Expected has {expected.Count} but results have {results.Count}");

            var mismatchDetected = false;
            int i = 0;
            foreach (var line in expected)
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

        private static bool ContainsEquivalent(HashSet<string> hs, string s)
        {
            foreach (var line in hs)
            {
                if (IsEquivalent(line, s))
                    return true;
            }
            return false;
        }

        private static bool IsEquivalent(string a, string b)
        {
            var splitA = a.Split(' ');
            var splitB = b.Split(' ');
            for (int i = 0; i < splitA.Length; i++)
            {
                if (!splitB.Contains(splitA[i]))
                    return false;
            }
            return true;
        }
    }
}
