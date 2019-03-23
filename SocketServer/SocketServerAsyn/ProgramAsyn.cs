using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServerAsyn
{
    class ProgramAsyn
    {
        private const int mPort = 11000;
        private const string ip = "127.0.0.1";
        static void Main(string[] args)
        {
            var client = new SocketServer(ip, mPort);
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
    public class SocketServer
    {

        private ManualResetEvent _acceptDone = new ManualResetEvent(false);

        private string _response = string.Empty;

        private AsyncCallback _acceptCallback;
        private AsyncCallback _sendCallback;
        private AsyncCallback _readCallback;

        public SocketServer(string ip, int port)
        {
            IPAddress ipAdd = IPAddress.Parse(ip);
            IPEndPoint localEP = new IPEndPoint(ipAdd, port);

            _acceptCallback = new AsyncCallback(OnAcceptCallback);
            _sendCallback = new AsyncCallback(OnSendCallback);
            _readCallback = new AsyncCallback(OnReadCallback);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Bind(localEP);
                server.Listen(100);
                while (true)
                {
                    _acceptDone.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    server.BeginAccept(_acceptCallback, server);
                    _acceptDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\n Press ENTER to continue...");
            Console.Read();
        }

        private void Send(Socket handler, string v)
        {
            byte[] byteData = Encoding.Default.GetBytes(v);
            handler.BeginSend(byteData, 0, byteData.Length, 0, _sendCallback, handler);
        }

        private void OnAcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket server = ar.AsyncState as Socket;
                Socket handler = server.EndAccept(ar);
                StateObject state = new StateObject();
                state._workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, _readCallback, state);
                _acceptDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void OnReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            StateObject state = ar.AsyncState as StateObject;
            Socket handler = state._workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} bytes form socket.\n Data:{1}", content.Length, content);
                    Send(handler, "Server return: " + content);
                }
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, _readCallback, state);
            }
        }

        private void OnSendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = ar.AsyncState as Socket;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
