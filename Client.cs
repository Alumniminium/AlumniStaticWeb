using System;
using System.Threading;
using System.Threading.Tasks;
using AlumniStaticWeb.IO;
using AlumniStaticWeb.Sockets;

namespace AlumniStaticWeb
{
    public class Client
    {
        public ClientSocket ClientSocket;
        internal WebFile File;
        internal long Offset;

        public Client(ClientSocket clientSocket) => ClientSocket = clientSocket;

        public Task ForceSend(byte[] content, int size) => ClientSocket.Send(content, size);

        public override string ToString() => ClientSocket.GetIP();
    }
}
