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
            ClientSocket socket = new ClientSocket();
            Console.ReadKey();
        }
    }
    class ClientSocket
    {
        private Socket _socket;
        public ClientSocket()
        {
            int port = 6000;
            string host = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ipe);
            Console.WriteLine("ClientSocket is Connecting!");

            string content = "send from client: hello";
            Console.WriteLine(content);
            byte[] sendBytes = Encoding.ASCII.GetBytes(content);
            _socket.Send(sendBytes, sendBytes.Length, 0);

            byte[] recBytes = new byte[4096];
            int bytes = _socket.Receive(recBytes, recBytes.Length, 0);
            string recStr = Encoding.ASCII.GetString(recBytes, 0, bytes);
            Console.WriteLine("Client Receive: " + recStr);

            _socket.Close();
        }
    }
}
