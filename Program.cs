using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AlumniStaticWeb.IO;
using AlumniStaticWeb.Sockets;

namespace AlumniStaticWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/WWW/";

            if (Debugger.IsAttached)
                Servers.Start(8888);
            else
                Servers.Start(80);

            foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories))
                MemoryCache.GetFile(file);

            TransferQueue.Start();

            while (true)
                Console.ReadLine();
        }
    }
}
