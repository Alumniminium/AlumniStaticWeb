using System;
using System.Threading;
using AlumniStaticWeb.Sockets;

namespace AlumniStaticWeb
{
    public class Client
    {
        public ClientSocket ClientSocket;
        internal string Path;
        internal int Offset;

        public Client(ClientSocket clientSocket) => ClientSocket = clientSocket;

        public void ForceSend(byte[] content, int size) => ClientSocket.Send(content, size);

        public override string ToString() => ClientSocket.GetIP();
    }
}
