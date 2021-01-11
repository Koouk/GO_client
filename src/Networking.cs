using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.Json;


namespace GOclient
{
    public enum ConnectionStatus : ushort
    {
        free,
        connecting,
        sending,
        sent,
        receiving,
        recieved
    }

    public class DataTemplate
    {
        public String Type { get; set; }
        public String Data { get; set; }

    }

    class Networking
    {
        private static char STOP = '\n';
        private static readonly int BUF_SIZE = 1024;
        private Socket _socket;
        private string _address;
        private int _port;

        public string RecData { get; set; }
        public bool IsConnected { get; private set; }
        public ConnectionStatus Status { get; set; }

        public int Error { get; private set; }
        public Networking(String address, int port)
        {
            Error = 0;
            _address = address;
            _port = port;
            IsConnected = false;
            Status = ConnectionStatus.free;
        }

        public DataTemplate GetData()
        {
            try
            {
                var data = JsonSerializer.Deserialize<DataTemplate>(RecData);
                Status = ConnectionStatus.free;
                Debug.WriteLine(" Recieved data: " + data.Data + " T: " + data.Type);
                return data;
            }
            catch (Exception exc)
            {
                Error = -1;
                Debug.WriteLine(" Wrong data format: " + exc.Message.ToString());
                return new DataTemplate { Type = "error", Data = "error" };
            }
        }
        public void Connect()
        {
            Status = ConnectionStatus.connecting;
            Dns.BeginGetHostByName(_address, new AsyncCallback(GetHostEntryCallback), null);

        }


        public void Send(String type, String data)
        {
            Status = ConnectionStatus.sending;

            var structure = new DataTemplate
            {
                Type = type,
                Data = data
            };
            try
            {
                string jsonString = JsonSerializer.Serialize(structure);

            Debug.WriteLine(" Send data: " + jsonString);
            SendBuffer buffer = new SendBuffer
            {
                buff = Encoding.ASCII.GetBytes(jsonString + "\n") 
            };

            
                _socket.BeginSend(buffer.buff, 0, buffer.buff.Length, 0, new AsyncCallback(SendCallback), buffer);
            }
            catch (Exception exc)
            {
                Error = -1;
                Debug.WriteLine(" Error while sending data: " + exc.Message.ToString());
            }
        }

        public void Receive()
        {
            Status = ConnectionStatus.receiving;
            RecData = "";
            RecBuffer buffer = new RecBuffer();
            try { 
                _socket.BeginReceive(buffer.buff, 0, BUF_SIZE, 0, new AsyncCallback(ReceiveCallback), buffer);
            }
            catch (Exception exc)
            {
                Error = -1;
                Debug.WriteLine(" Error while receiving data: " + exc.Message.ToString());
            }
}

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                RecBuffer data = (RecBuffer)ar.AsyncState;
                int count = _socket.EndReceive(ar);
                if (count == 0)
                {

                    Console.WriteLine(" Connection closed ");
                    return;

                }
                if (data.buff[count - 1] != STOP)
                {
                    data.rec.Append(Encoding.ASCII.GetString(data.buff, 0, count));

                    Debug.WriteLine(data.rec);
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
                Error = -1;
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
                    Status = ConnectionStatus.sent;
                }
            }
            catch (Exception exc)
            {
                Error = -1;

                Debug.WriteLine(" Error while sending data: " + exc.Message.ToString());

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
                Error = -1;
                Debug.WriteLine(" Error while creating socket: " + exc.Message.ToString());
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
                Error = -1;
                Debug.WriteLine("Error while connecting to serwer " + exc.Message.ToString());
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
