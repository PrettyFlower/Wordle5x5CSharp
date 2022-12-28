If you somehow got here and have no idea what's going on, it all started with this video from stand-up maths: https://www.youtube.com/watch?v=c33AZBnRHks&ab_channel=Stand-upMaths

To build:
1. Make sure you have dotnet 7.0
2. Update the file paths in Util.cs
3. Run the following where `<root>` is the root path of the repo and `<RID>` is your platform (e.g. win-x64, linux-x64).
```
cd <root>/Wordle5x5CSharp
dotnet publish -r <RID> -c Release
```

To test, make sure you use the generated code in the publish folder! Assuming you are using hyperfine, run:
```
hyperfine bin/Release/net7.0/win-x64/publish/Wordle5x5CSharp.exe
```

!!! IMPORTANT !!!  
Don't just run `dotnet build --configuration Release`! Because the benchmark only tests a single iteration, this does not give .NET enough time to apply JIT optimizations so we need to use ahead-of-time compiliation to get decent results. This makes an almost 5x performance difference on my own PC. For more information, see here: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/

This implementation just steals a bunch of good ideas from other smarter people and doesn't really have any creative ideas of its own. I just wanted to see how a C# version would compare. You can take a look at this spreadsheet here to see how other people solved it: https://docs.google.com/spreadsheets/d/11sUBkPSEhbGx2K8ah6WbGV62P8ii5l5vVeMpkzk17PI/edit#gid=0 At a high level, this code is probably closest to this Java version: https://github.com/Plexcalibur/5Words25Letters The optimizations used in this implementation are:
- [ ] Graph representation
- [x] Bitwise word representation
- [x] Anagram filtering
- [x] Words indexed by contained characters
- [x] Uses character frequency
- [ ] Pruning character sets that were already tried
- [x] Recursive
- [x] Parallelized
- [ ] Knuth's Algorithm X (I don't even know what this is)
