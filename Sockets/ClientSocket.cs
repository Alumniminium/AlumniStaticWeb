using System;
using System.Net;
using System.Net.Sockets;

namespace AlumniStaticWeb.Sockets
{
    public class ClientSocket : MarshalByRefObject
    {
        public bool Connected = true;
        public byte[] ReceiveBuffer = new byte[850];
        public int RecvSize;
        public Socket Socket;
        public Client Ref;
        private string IP;

        public ClientSocket(Socket socket)
        {
            Socket = socket;
            Socket.Blocking = true;
            Socket.NoDelay = true;
            Socket.DontFragment = true;
        }

        public void Disconnect()
        {
            try
            {
                Connected = false;
                ServerSocket.InvokeDisconnect(this);
            }
            finally
            {
                Socket?.Dispose();
                Socket = null;
            }
        }

        public void Send(byte[] packet, int size)
        {
            try
            {
                if (packet == null || Socket == null || !Connected)
                    return;

                Socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Disconnect();
            }
        }

        public void AsyncReceive(IAsyncResult ar)
        {
            ReceiveBuffer = (byte[])ar.AsyncState;
            if (Socket == null)
            {
                Disconnect();
                return;
            }
            try
            {
                RecvSize = Socket.EndReceive(ar, out var error);

                if (error == SocketError.Success && RecvSize > 0)
                {
                    var packet = new byte[RecvSize];
                    Buffer.BlockCopy(ReceiveBuffer, 0, packet, 0, RecvSize);
                    PacketHandler.Handle(Ref, packet);
                    Socket?.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, AsyncReceive, ReceiveBuffer);
                }
                else
                    Disconnect();
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                {
                    Console.WriteLine($"Disconnect Reason: {ex.Message} \r\n {ex.StackTrace}");
                    Console.WriteLine(ex);
                }

                Disconnect();
            }
        }

        public string GetIP()
        {
            if (string.IsNullOrEmpty(IP))
            {
                if (Socket == null)
                    return "";
                var remoteIpEndPoint = Socket?.RemoteEndPoint as IPEndPoint;
                IP = remoteIpEndPoint?.Address.ToString();
            }

            return IP;
        }
    }
}