using System;
using System.Net;
using System.Collections.Generic;

namespace Bopscotch.Communication
{
    public class CommunicationHandler
    {
        public delegate void CommsEventHandler(Dictionary<string, string> decodedData);

        private UdpAnySourceMulticastChannel _channel;

        public CommsEventHandler CommsCallback { get; set; }
        public string MyID { private get; set; }

        public CommunicationHandler()
        {
            _channel = new UdpAnySourceMulticastChannel(IPAddress.Parse("224.109.108.107"), 3007);
            _channel.PacketReceived += new EventHandler<UdpPacketReceivedEventArgs>(ChannelPacketReceived);

            CommsCallback = null;
        }

        private void ChannelPacketReceived(object sender, UdpPacketReceivedEventArgs e)
        {
            if (CommsCallback != null)
            {
                string data = e.Message;
                data = data.Trim('\0');

                if ((data.IndexOf(Game_Identifier) > -1) && (data.IndexOf(string.Concat("&id=", MyID)) < 0))
                {
                    DecodeAndSendData(data);
                }
            }
        }

        private void DecodeAndSendData(string encodedData)
        {
            Dictionary<string, string> decodedData = new Dictionary<string, string>();
            bool dataIsValid = true;

            foreach (string s in encodedData.Split('&'))
            {
                if (s.IndexOf("=") > -1)
                {
                    string key = s.Split('=')[0];
                    string value = s.Split('=')[1].Trim();

                    if ((!decodedData.ContainsKey(key)) && (!string.IsNullOrEmpty(value))) { decodedData.Add(key, value); }
                    else { dataIsValid = false; break; }
                }
                else
                {
                    dataIsValid = false; break;
                }
            }

            if (dataIsValid) { CommsCallback(decodedData); }
        }

        public void SendData(string message)
        {
            message = string.Concat(Game_Identifier, "&id=", MyID, "&", message);
            _channel.Send(message);
        }

        public void Open()
        {
            try { _channel.Open(); }
            catch { }
        }

        public void Close()
        {
            try { _channel.Close(); }
            catch { }
        }

        private const string Game_Identifier = "game=bopscotch";
    }
}
