using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AlumniStaticWeb.Sockets
{
    public class ServerSocket
    {
        public Action<Client, byte[]> OnReceive;
        public Socket Socket;
        
        public void Enable(ushort port)
        {
            Socket.Blocking = false;
            Socket.NoDelay = false;
            Socket.DontFragment = true;
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(50);
            Socket.BeginAccept(AsyncAccept, null);
            Console.WriteLine($"[{port}] PoolServer Started");
        }

        public static void InvokeDisconnect(ClientSocket clientSocket)
        {
            if (clientSocket == null)
                return;

            if (clientSocket.Socket !=null && clientSocket.Socket.Connected)
                clientSocket.Socket.Close();
            else
                clientSocket.Ref = null;
        }

        private async void AsyncAccept(IAsyncResult res)
        {
            try
            {
                await Task.Delay(10);
                var client = new ClientSocket(Socket.EndAccept(res));
                client.Ref = new Client(client);
                Socket.BeginAccept(AsyncAccept, null);
                client.Socket.BeginReceive(client.ReceiveBuffer, 0, client.ReceiveBuffer.Length, SocketFlags.None, client.AsyncReceive, client.ReceiveBuffer);
            }
            catch (Exception e)
            {
                Socket.BeginAccept(AsyncAccept, null);
                Console.WriteLine(e);
            }
        }
    }
}