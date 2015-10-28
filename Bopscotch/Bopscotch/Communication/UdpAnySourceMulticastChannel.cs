using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Bopscotch.Communication
{
    public class UdpAnySourceMulticastChannel : IDisposable
    {
        public event EventHandler<UdpPacketReceivedEventArgs> PacketReceived;
        public event EventHandler AfterOpen;
        public event EventHandler BeforeClose;
        public bool IsDisposed { get; private set; }
        public static bool IsJoined;
        private byte[] ReceiveBuffer { get; set; }
        private UdpAnySourceMulticastClient Client { get; set; }

        public UdpAnySourceMulticastChannel(IPAddress address, int port)
            : this(address, port, 1024)
        { }

        public UdpAnySourceMulticastChannel(IPAddress address, int port, int maxMessageSize)
        {
            this.ReceiveBuffer = new byte[maxMessageSize];
            this.Client = new UdpAnySourceMulticastClient(address, port);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                this.IsDisposed = true;

                if (this.Client != null) { this.Client.Dispose(); }
            }
        }

        public void Open()
        {
            if (!IsJoined)
            {
                this.Client.BeginJoinGroup(
                    result =>
                    {
                        try
                        {
                            this.Client.EndJoinGroup(result);
                            IsJoined = true;

                            this.OnAfterOpen();
                            this.Receive();
                        }
                        catch (Exception ex)
                        { }
                    }, null);
            }
        }

        public void Close()
        {
            this.OnBeforeClose();
            IsJoined = false;
            this.Dispose();
        }

        public void Send(string format, params object[] args)
        {
            if (IsJoined)
            {
                byte[] data = Encoding.UTF8.GetBytes(string.Format(format, args));

                this.Client.BeginSendToGroup(data, 0, data.Length,
                    result =>
                    {
                        this.Client.EndSendToGroup(result);
                    }, null);
            }
        }

        public void SendTo(IPEndPoint endPoint, string format, params object[] args)
        {
            if (IsJoined)
            {
                byte[] data = Encoding.UTF8.GetBytes(string.Format(format, args));

                this.Client.BeginSendTo(data, 0, data.Length, endPoint,
                    result =>
                    {
                        this.Client.EndSendToGroup(result);
                    }, null);
            }
        }

        private void Receive()
        {
            if (IsJoined)
            {
                Array.Clear(this.ReceiveBuffer, 0, this.ReceiveBuffer.Length);

                this.Client.BeginReceiveFromGroup(this.ReceiveBuffer, 0, this.ReceiveBuffer.Length,
                    result =>
                    {
                        if (!IsDisposed)
                        {
                            IPEndPoint source;

                            try
                            {
                                this.Client.EndReceiveFromGroup(result, out source);
                                this.OnReceive(source, this.ReceiveBuffer);
                                this.Receive();
                            }
                            catch
                            {
                                IsJoined = false;
                                this.Open();
                            }

                        }
                    }, null);
            }
        }

        private void OnReceive(IPEndPoint source, byte[] data)
        {
            EventHandler<UdpPacketReceivedEventArgs> handler = this.PacketReceived;

            if (handler != null) { handler(this, new UdpPacketReceivedEventArgs(data, source)); }
        }

        private void OnAfterOpen()
        {
            EventHandler handler = this.AfterOpen;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        private void OnBeforeClose()
        {
            EventHandler handler = this.BeforeClose;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }
    }
}