using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AlumniStaticWeb.IO;

namespace AlumniStaticWeb.Sockets.HTTP
{
    public static class PacketHandler
    {
        internal static void Handle(Client client, byte[] packet)
        {
            var request = Encoding.UTF8.GetString(packet).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var requestUri = request[0].Split(' ')[1].Split('?')[0];

            foreach (var lin in request)
            {
                if (lin.Contains("Range"))
                {
                    var line = lin.Replace("Range:", "").Trim();
                    client.Offset = long.Parse(line.Replace("bytes=", "").Replace("-", ""));
                    break;
                }
            }

            if (requestUri.EndsWith("/"))
                requestUri += "index.html";
            if (requestUri.StartsWith("/"))
                requestUri = requestUri.Remove(0, 1);

            var Path = Environment.CurrentDirectory + "/" + requestUri;

            var cacheFile = MemoryCache.GetFile(requestUri);
            if (cacheFile != null)
            {
                client.File = cacheFile;
                TransferQueue.Add(client);
            }
        }
        public static byte[] GenerateHeader(int statusCode, string contentType, string contentEncoding = "gzip")
        {
            var header = "HTTP/1.1 " + statusCode + " OK" + "\r\n";
            header += "Server: AlumniStaticWebServer 1.0\r\n";
            header += "Content-Type: " + contentType + "; charset=utf-8\r\n";
            header += "Accept-Ranges: none\r\n";
            header += "Content-Encoding: " + contentEncoding + "\r\n";
            header += "Cache-Control: max-age=2592000\r\n";
            header += "Access-Control-Allow-Origin: *\r\n";
            header += "Transfer-Encoding: chunked\r\n\r\n";

            return Encoding.UTF8.GetBytes(header);
        }
    }
}