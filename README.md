If you somehow got here and have no idea what's going on, it all started with this video from stand-up maths: https://www.youtube.com/watch?v=c33AZBnRHks&ab_channel=Stand-upMaths

To build:
```
cd <root>/Wordle5x5CSharp
dotnet build --configuration Release
```

To run:
```
dotnet bin\Release\net6.0\Wordle5x5CSharp.dll
```

The number of iterations can be specified as a command line argument (default is 10):
```
dotnet bin/Release/net6.0/Wordle5x5CSharp.dll 5
```

On my own PC, I'm getting results around here (times are measured in milliseconds):
```
Average: 28, min: 26, max: 42
```

!!! IMPORTANT !!!
When testing single iteration passes with hyperfine, you may get misleading results as this doesn't give .NET enough time to use JIT optimizations. For better results, please run:
```
dotnet publish -r <RID> -c Release
```
Where <RID> is your platform (e.g. win-x64, linux-x64). This will allow for the JIT optimizations to be baked into the application and can result in a nearly 2x performance improvement. For more information, see here: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/

This implementation just steals a bunch of good ideas from other smarter people and doesn't really have any creative ideas of its own. I just wanted to see how a C# version would compare. You can take a look at this spreadsheet here to see how other people solved it: https://docs.google.com/spreadsheets/d/11sUBkPSEhbGx2K8ah6WbGV62P8ii5l5vVeMpkzk17PI/edit#gid=0 At a high level, this code is probably closest to this Java version: https://github.com/Plexcalibur/5Words25Letters The optimizations used in this implementation are:
- [ ] Graph representation
- [x] Bitwise word representation
- [x] Anagram filtering
- [x] Words indexed by contained characters
- [x] Uses character frequency
- [ ] Pruning character sets that were already tried
- [x] Recursive
- [ ] Parallelized
- [ ] Knuth's Algorithm X (I don't even know what this is)
