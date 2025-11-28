using System;
using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.Tests;

[LuaProgram]
public static class Vector3Test
{
    public static Vector3 testVector { get; set; } = new(5, 3, 2);
    
    [LuaMain]
    public static void Start()
    {
        Console.WriteLine("(" + testVector + ")");
        Console.WriteLine(testVector + new Vector3(4, 3, 1));
        Console.WriteLine(testVector * 4);
        Console.WriteLine(testVector / 2);
        Console.WriteLine(-testVector);
        Console.WriteLine(testVector == new Vector3(5, 3, 2));
        Console.WriteLine(testVector == new Vector3(4, 3, 1));
        Console.WriteLine(new Vector3());
    }
}