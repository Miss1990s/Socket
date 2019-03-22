using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket server = new ServerSocket();
            Console.ReadKey();
        }
    }
    class ServerSocket
    {
        private Socket _gateSocket;
        private Socket _contentSocket;
        public ServerSocket()
        {
            int port = 6000;
            string host = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            _gateSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _gateSocket.Bind(ipe);
            _gateSocket.Listen(0);
            Console.WriteLine("ServerSocket is Listening!");

            _contentSocket = _gateSocket.Accept();
            Console.WriteLine("Client is Accept");

            byte[] recBytes = new byte[4096];
            int bytes = _contentSocket.Receive(recBytes, recBytes.Length, 0);
            string recStr = Encoding.ASCII.GetString(recBytes, 0, bytes);
            Console.WriteLine("Server Receive: " + recStr);

            string content = "send from server: hello world";
            Console.WriteLine(content);
            byte[] sendBytes = Encoding.ASCII.GetBytes(content);
            _contentSocket.Send(sendBytes, sendBytes.Length, 0);
            _contentSocket.Close();
            _gateSocket.Close();
        }
    }
}
