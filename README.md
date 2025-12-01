# <img src="./Logo.png" width="50"> CCSharp
CCSharp allows you to write ComputerCraft programs in C# by transpiling them into Lua.


```C#
[LuaProgram]
public static class LogKeyEvents
{
    [LuaMain]
    public static void Start()
    {
        while (true)
        {
            KeyEvent keyEvent = OS.PullEvent<KeyEvent>();
            Console.WriteLine(Keys.GetName(keyEvent.Key) + " held=" + keyEvent.Held);
        }
    }
}
```
```LUA
function Start()
 while true do
  local keyEvent = {os.pullEvent("key")};
  print((tostring(keys.getName(keyEvent[2])).." held=")..tostring(keyEvent[3]))
 end
end
Start()
```

## Docs
* [How to use](./docs/how_to_use.md)
* [Quirks to be aware of](./docs/quirks.md)



## TODO
- Properly handle local variable names that share names with lua apis or lua keywords by automatically renaming them to something safe.
- Add support for all Math functions
- Update to newer version of ICSharpCode.Decompiler
- Fix string concatenation (this will happen with the ICSharpCode.Decompiler update)
- Compile get and set accessors for non auto implemented properties (need to decide if I want to use \_\_index and \_\_newindex to mimic how it works in C# or just generate regular get_* and set_* methods)
- A compile flag on [LuaProgram] to have all dependent [LuaClass] be compiled as modules
- try catch support via xpcall
- Support for all Linq functions
- Improve documentation
- Support for [LuaClass] on structs by generating a copy method that gets called automatically when passing by value
- An option to preserve local variable names by providing pdb to ICSharpCode.Decompiler
- Support multiple operator overloads with varying types (ex: a vector that can be multiplied by both another vector or a scalar value)
- Add proper unit tests to CCSharp.Tests that get run in CraftOS-PC
- Create adapter classes for the full computercraft api
- Create adapter classes for various peripheral mod apis (if you PR these separate each mod intos its own namespace)
#### Currently supported linq functions
- ToArray
- ToList
- GroupBy
- OrderBy
- OrderByDescending
- Where
- Select
- Distinct
- Take
- First
- FirstOrDefault
- Count
- LongCount
- Any
- Sum
#### Things I'm not planning on adding support for (but would welcome a PR)
- out parameters
- async
- Reflection

## Source Guide

[CCSharp.Demo](./tests/CCSharp.Demo) contains a few demo programs to test various functionality.

[CCSharp.Tests](./tests/CCSharp.Tests) contains a few test programs, eventually this will run proper unit tests.

### C# Code Decompilation
CCSharp uses [ICSharpCode.Decompiler](https://www.nuget.org/packages/ICSharpCode.Decompiler) to decompile C# programs into a C# syntax tree.

### RedIL
[RedIL](./src/CCSharp/RedIL) is an intermediate language that is created from decompiled C# syntax tree, and later compiled to Lua. It's name comes from the original library this was forked from, [RediSharp](https://github.com/areller/RediSharp)

### Lua Compilation
Lua is written by traversing the RedIL using an [IRedILVisitor](./src/CCSharp/RedIL/IRedILVisitor.cs).  
See the [Lua](./src/CCSharp/Lua) folder.

### Adapter Classes
All of the classes that actually adapt the ComputerCraft api can be found in the [ComputerCraft](./src/CCSharp/ComputerCraft) namespace

### Dependencies & Attributions
- [ICSharpCode.Decompiler](https://www.nuget.org/packages/ICSharpCode.Decompiler) which is the decompiler engine built for ILSpy
- [RediSharp](https://github.com/areller/RediSharp) Project is based on and uses it's RedIL but further expanded to handle full programs, classes, and better adapt exising lua apis.