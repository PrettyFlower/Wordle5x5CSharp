using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle5x5CSharp
{
    public static class LetterFrequency
    {
        public static void Find()
        {
            var letterFreq = new (char letter, int count)[26];
            for(int i = 0; i < letterFreq.Length; i++)
            {
                letterFreq[i].letter = (char)(i + 'a');
            }
            var file = @"C:\code\Wordle5x5CSharp\Wordle5x5CSharp\words_alpha.txt";
            using (var sr = new StreamReader(file))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != 5)
                        continue;
                    foreach(var c in line)
                    {
                        letterFreq[c - 'a'].count++;
                    }
                }
            }
            var ordered = letterFreq.OrderBy(l => l.count).ToArray();
            foreach(var (letter, count) in ordered)
            {
                Console.WriteLine($"{letter}: {count}");
            }
            foreach(var (letter, _) in ordered)
            {
                Console.Write(letter);
            }
            Console.WriteLine();
        }
    }
}
