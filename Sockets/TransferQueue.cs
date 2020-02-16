using System;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace AlumniStaticWeb.Sockets
{
    public static class TransferQueue
    {
        public static Thread Worker;
        public static ConcurrentQueue<Client> Queue = new ConcurrentQueue<Client>();
        public static AutoResetEvent Block = new AutoResetEvent(false);
        public static void Start()
        {
            Worker = new Thread(Loop);
            Worker.Start();
        }

        public static void Add(Client c)
        {
            Queue.Enqueue(c);
            Block.Set();
        }

        public static void Loop()
        {
            while (true)
            {
                if (Queue.TryDequeue(out var client))
                {
                    using (var stream = File.OpenRead(client.Path))
                    {
                        stream.Seek(client.Offset, SeekOrigin.Begin);
                        var buffer = new byte[1024*625];
                        int readBytes = stream.Read(buffer, 0, buffer.Length);
                        client.Offset += readBytes;
                        Console.WriteLine($"Creating chunk from {stream.Position-readBytes} to {stream.Position} for {System.IO.Path.GetFileName(client.Path)}");
                        var chunk = Chunk.Create(buffer, readBytes);
                        client.ForceSend(chunk, chunk.Size);

                        if (stream.Position == stream.Length)
                        {
                            var lastChunk = Chunk.CreateLast();
                            client.ForceSend(lastChunk, lastChunk.Size);
                            client.ClientSocket.Disconnect();
                        }
                    }
                    if (client.ClientSocket.Connected)
                        Add(client);
                }
                Thread.Sleep(1);
            }
        }
    }
}