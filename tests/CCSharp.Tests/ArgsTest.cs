using System;
using CCSharp.Attributes;

namespace CCSharp.Tests;

[LuaProgram]
public static class ArgsTest
{
    [LuaMain]
    public static void Start(string[] arguments)
    {
        Console.WriteLine("Arguments:");
        foreach (string argument in arguments)
        {
            Console.WriteLine(argument);
        }
    }
}