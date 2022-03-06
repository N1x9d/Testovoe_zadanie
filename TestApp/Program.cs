using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{

    /// <summary>
    /// Server logic
    /// </summary>
    public class Program
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);


        private static int _reqestLimit = 0;
        private static int _curInProcess = 0;

        public static int Main(String[] args)
        {
            Console.Title = "Server";

            Console.WriteLine("Введите предел одновременных запросов n");
            if (!int.TryParse(Console.ReadLine(), out _reqestLimit))
                Console.WriteLine("Неверный формат ввода");
            else
                StartListening();
            return 0;
        }

        

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(1000);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                
                    _curInProcess++;
                    if (_curInProcess > _reqestLimit)
                    {
                        Console.WriteLine(" Received Text: " + content + " ServerOverLoad");
                    // server.SendMoreFrame(routingKey);
                        Send(handler, "ServerOverLoad");
                        _curInProcess--;
                    }
                    else
                    {
                        Console.WriteLine(" Received Text: " + content);
                        var res = IsPalindrom(content);
                        if (res)
                        {
                        Console.WriteLine(" Received Text: " + content + " Palindrom");
                        //   server.SendMoreFrame(routingKey);
                       
                        Send(handler, "Palindrom");
                        }
                        else
                        {
                        Console.WriteLine(" Received Text: " + content + "not Palindrom");
                        // server.SendMoreFrame(routingKey);
                        Send(handler, "Not palindrom");
                        }   
                        _curInProcess--;
                    }
                    //Send(handler, content);
               
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static bool IsPalindrom(string Input)
        {
            Thread.Sleep(1000);
            var s = RemovePunctuation(Input).ToCharArray();

            for (int i = 0, j = s.Length-1; i < j; i++, j--)

                if (s[i] != s[j])
                    return false;
            return true;
        }

        private static string RemovePunctuation(string Inp)
        {
            string Resalt = Inp;
            string[] punctuation_marks = { ".", ",", "!", "?", ";", ":", "-", " " };
            //убираем знаки припенания
            foreach (var punct in punctuation_marks)
            {
                Resalt.Replace(punct[0], ' ');
            }
            //убираем пробелы
            while (Resalt.Contains("  ")) 
            { 
                Resalt = Resalt.Replace("  ", ""); 
            }
            return Resalt;          
        }
    }
}
