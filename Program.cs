using Delta.Core;
using Delta.Network;
using System;

public class Program
{
    public static void Main()
    {
        new ClientHandler(null, null);

        ProxyManager pm = new ProxyManager();
        //pm.Start();

        Console.ReadLine();
    }
}