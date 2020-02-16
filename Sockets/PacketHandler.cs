using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AlumniStaticWeb.IO;

namespace AlumniStaticWeb.Sockets
{
    public static class PacketHandler
    {
        internal static void Handle(Client client, byte[] packet)
        {
            var sw = Stopwatch.StartNew();
            int statusCode = 200;
            var request = Encoding.UTF8.GetString(packet).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var requestUri = request[0].Split(' ')[1].Split('?')[0];
            int range = 0;
            foreach (var lin in request)
            {
                if (lin.Contains("Range"))
                {
                    var line = lin.Replace("Range:", "").Trim();
                    range = int.Parse(line.Replace("bytes=", "").Replace("-", ""));
                    break;
                }
            }
            if (requestUri.EndsWith("/"))
                requestUri += "index.html";
            if (requestUri.StartsWith("/"))
                requestUri = requestUri.Remove(0, 1);
            if (requestUri.EndsWith("ip"))
            {
                var response = Encoding.UTF8.GetBytes(client.ClientSocket.GetIP());
                statusCode = 206;
                var header = GenerateHeader(statusCode, "text/plain", "Identity");
                client.ForceSend(header, header.Length);
                client.ForceSend(response, response.Length);
                return;
            }

            var Path = Environment.CurrentDirectory + "/" + requestUri;
            if (File.Exists(Environment.CurrentDirectory + "/" + requestUri))
            {
                using (var stream = File.OpenRead(Path))
                {
                    var mimeType = "text/plain";
                    if (MimeTypes.Mappings.TryGetValue(System.IO.Path.GetExtension(Path), out var mimetype))
                        mimeType = mimetype;

                    if (range == 0)
                    {
                        var header = GenerateHeader(200, mimetype, "Identity");
                        client.ForceSend(header, header.Length);
                    }
                    client.Path = Path;
                    client.Offset = range;
                    TransferQueue.Add(client);
                }
                sw.Stop();
                Console.WriteLine("[" + client + "] in " + sw.Elapsed.TotalMilliseconds + "ms (" + statusCode + ")-> " + requestUri);

            }
        }
        private static byte[] GenerateHeader(int statusCode, string contentType, string contentEncoding = "gzip")
        {
            var header = "HTTP/1.1 " + statusCode + " OK" + "\r\n";
            header += "Server: AlumniStaticWebServer 1.0\r\n";
            header += "Content-Type: " + contentType + "; charset=utf-8\r\n";
            header += "Accept-Ranges: none\r\n";
            header += "Content-Encoding: " + contentEncoding + "\r\n";
            header += "Cache-Control: max-age=2592000\r\n";
            header += "Transfer-Encoding: chunked\r\n\r\n";

            return Encoding.UTF8.GetBytes(header);
        }
    }
}