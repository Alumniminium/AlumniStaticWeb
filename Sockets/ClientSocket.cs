using System;
using System.Net;
using System.Net.Sockets;
using AlumniStaticWeb.Sockets.HTTP;

namespace AlumniStaticWeb.Sockets
{
    public class ClientSocket : MarshalByRefObject
    {
        public bool Connected = true;
        public byte[] ReceiveBuffer = new byte[1024]; // 1kb per client should do, I don't support POST anyways
        public int RecvSize;
        public Socket Socket;
        public Client Ref;
        private string IP;

        public ClientSocket(Socket socket)
        {
            Socket = socket;
            Socket.Blocking = false;
            Socket.NoDelay = true; // I have a TransferQueue
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
            try
            {
                ReceiveBuffer = (byte[])ar.AsyncState;
                RecvSize = Socket.EndReceive(ar, out var error);

                if (error == SocketError.Success && RecvSize > 0)
                {
                    // surprisingly that's faster than Array.Copy/Buffer.BlockCopy
                    var packet = new byte[RecvSize];
                    ReceiveBuffer.AsSpan().Slice(0, RecvSize).CopyTo(packet.AsSpan());
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
                    return null;
                var remoteIpEndPoint = Socket?.RemoteEndPoint as IPEndPoint;
                IP = remoteIpEndPoint?.Address.ToString();
            }

            return IP;
        }
    }
}