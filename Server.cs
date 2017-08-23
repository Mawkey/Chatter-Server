using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ChatterServer
{
    class Server
    {
        /* 
        1. Server asks for port.
        2. Server starts listening for connections.
        3. Create new user object and pass the tcp connection to it.
        4. Show message upon successful connection.
        */

        //Fields
        TcpClient client;
        TcpListener serverListener;
        IPAddress ipAdress;
        Thread listenForConnections;
        Thread writeServerMessage;
        public static List<User> userList;
        string ipString;
        string portString;
        int port;


        //Properties

        //Constructor
        public Server()
        {
            userList = new List<User>();
            //Set IP Adress to that of the local machine.
            ipString = GetLocalAdress();
            if (IPAddress.TryParse(ipString, out ipAdress))
            {
                Console.Title = ipString;
            }

            //Asks user for port.
            while (!Int32.TryParse(portString, out port))
           {
                Console.Write("--Chat Server--"
            + "\nEnter your desired port the server will run on."
            + "\nEnter nothing for default port (54545)."
            + "\nPort: ");
                portString = Console.ReadLine();

                if (String.IsNullOrEmpty(portString))
                {
                    portString = "54545";
                    Console.Clear();
                }
            }
            Console.Title += ":" + port.ToString();

            //Initialize the listener.
            serverListener = new TcpListener(ipAdress, port);
            //Start listening at the specified port.
            serverListener.Start();

            Console.WriteLine("The server is running at port {0}...\n"
                            + "The local adress is: {1}\n\n"
                            + "Waiting for a connection..."
                            , port.ToString()
                            , serverListener.LocalEndpoint);
            listenForConnections = new Thread(new ThreadStart(ListenForConnections));
            listenForConnections.Start();

            writeServerMessage = new Thread(new ThreadStart(ChatFromServer));
            writeServerMessage.Start();

        }

        private void ChatFromServer()
        {
            string message;
            while(true)
            {
                message = Console.ReadLine();
                ServerMessage(message);
                message = "";
            }
        }

        private void ListenForConnections()
        {
            while (true)
            {
                if (serverListener.Pending())
                {
                    //if there are pending requests create a new connection
                    client = serverListener.AcceptTcpClient();
                    Console.WriteLine("Connection accepted");
                    User user = new User(client);
                }

            }
        }

        public static void SendMessageToClients(string message)
        {
            StreamWriter writer;

            for (int i = 0; i < userList.Count(); i++)
            {
                try
                {
                    if (message.Trim() == "" || userList[i] == null)
                    {
                        continue;
                    }

                    writer = new StreamWriter(userList[i].Client.GetStream());
                    writer.Write(message + "\r\n");
                    writer.Flush();
                }
                catch
                {

                }
            }
            writer = null;

        }

        public static void ServerMessage(string message)
        {
            StreamWriter writer;

            for (int i = 0; i < userList.Count(); i++)
            {
                try
                {
                    if (message.Trim() == "" || userList[i] == null)
                    {
                        continue;
                    }

                    writer = new StreamWriter(userList[i].Client.GetStream());
                    writer.Write("[SERVER]: " + message + "\r\n");
                    writer.Flush();
                }
                catch
                {

                }
            }
            writer = null;

        }

        private string GetLocalAdress()
        {
            string hostName = Dns.GetHostName();

            string IP = Dns.GetHostEntry(hostName).AddressList[2].ToString();
            return IP;

        }

    }
}
