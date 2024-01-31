using Delta.Core;
using Delta.Network;
using Delta.Tools;
using System;

public class Program
{
    public static void Main()
    {
        //new ClientHandler(null, null);

        float a = 0;

        Task.Run(() => 
        {
            while (true) 
            { 
                DLM.TryLock(ref a);
                a++;
                DLM.RemoveLock(ref a);

                //Console.WriteLine(a);

                Task.Delay(1).Wait();
            }
        });

        Task.Run(() =>
        {
            while (true)
            {
                DLM.TryLock(ref a);
                a += 0.1f;
                DLM.RemoveLock(ref a);
                //Console.WriteLine(a);

                Task.Delay(1).Wait();
            }
        });

        //ProxyManager pm = new ProxyManager();
        //pm.Start();

        Console.ReadLine();
    }
}