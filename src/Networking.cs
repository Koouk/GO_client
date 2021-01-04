using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace GOclient
{
    public enum ConnectionStatus : ushort
    {
        free,
        connecting,
        sending,
        receiving,
        recieved
    }

    class networking
    {
        private static readonly int BUF_SIZE = 1024;
        private Socket _socket;
        private String _address;
        private int _port;

        public string RecData { get; set; }
        public bool IsConnected { get; private set; }
        public ConnectionStatus Status { get;  set; }

        public networking(String address, int port)
        {
            _address = address;
            _port = port;
            IsConnected = false;
            Status = ConnectionStatus.free;
        }


        public void connect()
        {
            Status = ConnectionStatus.connecting;
            Dns.BeginGetHostByName(_address, new AsyncCallback(GetHostEntryCallback), null);

        }


        public void send(String toSend)
        {
            Status = ConnectionStatus.sending;
            SendBuffer data = new SendBuffer();
            data.buff = Encoding.ASCII.GetBytes(toSend);
            _socket.BeginSend(data.buff, 0, data.buff.Length, 0, new AsyncCallback(SendCallback), data);
        }

        public void receive()
        {
            Status = ConnectionStatus.receiving;
            RecBuffer buffer = new RecBuffer();
            _socket.BeginReceive(buffer.buff, 0, BUF_SIZE, 0, new AsyncCallback(ReceiveCallback), buffer);

        }

        public void close()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                RecBuffer data = (RecBuffer)ar.AsyncState;
                int count = _socket.EndReceive(ar);
                if(count == 0)
                {

                    Console.WriteLine(" Connection closed " );

                }
                if (data.buff[count - 1] != '\n')
                {
                    data.rec.Append(Encoding.ASCII.GetString(data.buff, 0, count));
                    _socket.BeginReceive(data.buff, 0, BUF_SIZE, 0, new AsyncCallback(ReceiveCallback), data);
                }
                else
                {
                    data.rec.Append(Encoding.ASCII.GetString(data.buff, 0, count - 1));
                    RecData = data.rec.ToString();
                    Status = ConnectionStatus.recieved;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(" Error while receiving data: " + exc.Message.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                SendBuffer sentData = (SendBuffer)ar.AsyncState;
                sentData.offset += _socket.EndSend(ar);

                if (sentData.offset < sentData.buff.Length)
                {
                    _socket.BeginSend(sentData.buff, sentData.offset, sentData.buff.Length - sentData.offset, 0, new AsyncCallback(SendCallback), sentData);
                }
                else
                {
                    Status = ConnectionStatus.free;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(" Error while sending data: " + exc.Message.ToString());
                
            }
        }

        private void GetHostEntryCallback(IAsyncResult ar)
        {
            try
            {
                IPHostEntry hostEntry = null;
                IPAddress[] addresses = null;
                IPEndPoint endPoint = null;
                Socket socket = null;


                hostEntry = Dns.EndGetHostEntry(ar);
                addresses = hostEntry.AddressList;
                endPoint = new IPEndPoint(addresses[0].MapToIPv4(), _port);


                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), socket);

            }
            catch (Exception exc)
            {
                Console.WriteLine(" Error while creating socket: " + exc.Message.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _socket = (Socket)ar.AsyncState;
                _socket.EndConnect(ar);
                IsConnected = true;
                Status = ConnectionStatus.free;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error while connecting to serwer "+ exc.Message.ToString());
                _socket = null;
            }
        }


        private class SendBuffer
        {
            public byte[] buff;
            public int offset = 0;
        }

        private class RecBuffer
        {
            public byte[] buff = new byte[BUF_SIZE];
            public StringBuilder rec = new StringBuilder();
        }


    }
}
