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
            {
                try
                {
                    Servers.Start(80);
                }
                catch
                {
                    Servers.Start(8888);
                }
            }
            foreach (var enumerateFileSystemEntry in Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories))
                Cache.GetFile(enumerateFileSystemEntry);

            TransferQueue.Start();
            
            while (true)
                Console.ReadLine();
        }
    }
}
