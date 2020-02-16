using System;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using AlumniStaticWeb.Sockets.HTTP;

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

        public static async void Loop()
        {
            while (true)
            {
                try
                {
                    if (Queue.TryDequeue(out var client))
                    {
                        using (var stream = new MemoryStream(client.File.Content))
                        {
                            if (client.Offset == 0)
                            {
                                var header = PacketHandler.GenerateHeader(200, client.File.MimeType);
                                await client.ForceSend(header, header.Length);
                            }
                            stream.Seek(client.Offset, SeekOrigin.Begin);
                            var buffer = new byte[1024 * 1024];
                            int readBytes = stream.Read(buffer, 0, buffer.Length);
                            client.Offset += readBytes;

                            var chunk = Chunk.Create(buffer, readBytes);
                            await client.ForceSend(chunk, chunk.Size);


                            if (stream.Position == stream.Length)
                            {
                                var lastChunk = Chunk.CreateLast();
                                await client.ForceSend(lastChunk, lastChunk.Size);
                                client.ClientSocket.Disconnect();
                            }
                        }
                        if (client.ClientSocket.Connected)
                            Add(client);
                    }
                }
                catch { }
                Thread.Sleep(1);
            }
        }
    }
}