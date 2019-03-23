using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClinentAsyn
{
    class ProgramAsyn
    {
        private const int mPort = 11000;
        private const string ip = "127.0.0.1";
        static void Main(string[] args)
        {
            var client = new SocketClient(ip, mPort);
            Console.ReadKey();
        }
    }
    public class StateObject
    {
        public Socket _workSocket = null;
        public const int BUFFER_SIZE = 256;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
    }
    public class SocketClient
    {
        
        private ManualResetEvent _connectDone = new ManualResetEvent(false);
        private ManualResetEvent _sendDone = new ManualResetEvent(false);
        private ManualResetEvent _receiveDone = new ManualResetEvent(false);
        private string _response = string.Empty;
        private AsyncCallback _connectCallback;
        private AsyncCallback _receiveCallback;
        private AsyncCallback _sendCallback;

        public SocketClient(string ip, int port)
        {
            try
            {
                IPAddress ipAdd = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAdd, port);
                _connectCallback = new AsyncCallback(OnConnectCallback);
                _receiveCallback = new AsyncCallback(OnReceiveCallback);
                _sendCallback = new AsyncCallback(OnSendCallback);
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEP, _connectCallback, client);
                _connectDone.WaitOne();
                string input = "This is a test<EOF>";
                Send(client,input );
                _sendDone.WaitOne();
                Receive(client);
                _receiveDone.WaitOne();

                Console.WriteLine("Response received :{0}", _response);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state._workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, _receiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, string v)
        {
            byte[] byteData = Encoding.Default.GetBytes(v);
            client.BeginSend(byteData, 0, byteData.Length, 0, _sendCallback, client);
        }

        private void OnConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                _connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void OnReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = ar.AsyncState as StateObject;
                Socket client = state._workSocket;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, _receiveCallback,state);
                }

                if (state.sb.Length > 1)
                {
                    _response = state.sb.ToString();
                }
                _receiveDone.Set();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                _receiveDone.Set();
            }
        }

        private void OnSendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = ar.AsyncState as Socket;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                _sendDone.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
