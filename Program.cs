using Delta.Core;
using System;

public class Program
{
    public static void Main()
    { 
        ProxyManager pm = new ProxyManager();
        pm.Start();

        Console.ReadLine();
    }
}