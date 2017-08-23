using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatterServer
{
    class User
    {
        //Fields
        TcpClient client;
        StreamWriter writer;
        StreamReader reader;
        string userName;

        //Properties
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public TcpClient Client
        {
            get { return client; }
            set { client = value; }
        }
        
        //Constructor
        public User(TcpClient client)
        {
            //Users personal client connection.
            this.client = client;
            Thread userThread = new Thread(new ThreadStart(StartChat));
            userThread.Start();
        }


        private void StartChat()
        {
            //Create our writer and reader.
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());

            bool userNameTaken;
            do
            {
                //Get the user name from client.
                userName = GetUserName();

                //Check if the user name is taken.
                userNameTaken = IsUserNameTaken(userName);

                if (userNameTaken)//If user name is taken...
                {
                    writer.Write("true" + "\r\n");
                }
                else//If user name is NOT taken...
                {
                    writer.Write("false" + "\r\n");
                }
                writer.Flush();
            } while (userNameTaken);

            Server.userList.Add(this);
            Console.WriteLine("{0} was added to the list. Count: {1}", 
                               userName, 
                               AmountOfUsersLeft());

            Thread userThread = new Thread(new ThreadStart(RunChat));
            userThread.Start();
        }

        /// <summary>
        /// Checks if the given user name is taken by another client.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private bool IsUserNameTaken(string userName)
        {
            bool userNameTaken = false;
            //Check if user name already exists.
            for (int i = 0; i < Server.userList.Count(); i++)
            {
                //If the chosen user name is taken...
                if (Server.userList[i].userName == this.userName)
                {
                    userNameTaken = true;
                    break;
                }
            }
            return userNameTaken;
        }

        /// <summary>
        /// Waits for the client to send a message, then send it to everyone.
        /// </summary>
        private void RunChat()
        {
            try
            {
                Console.WriteLine(userName + " connected.");
                //set out line variable to an empty string
                string line = "";
                while (true)
                {
                    //read the curent line
                    line = reader.ReadLine();
                    //send our message to console.
                    Console.WriteLine(line);
                    //Send our message to every client.
                    Server.SendMessageToClients(line);
                }
            }
            catch
            {
                Server.userList.Remove(this);
                Console.WriteLine("{0} disconnected. Users remaining: {1}", userName, AmountOfUsersLeft());
            }
        }

        /// <summary>
        /// Reads the current stream for a user name and returns it.
        /// </summary>
        /// <returns></returns>
        private string GetUserName()
        {
            return reader.ReadLine();
        }

        /// <summary>
        /// Returns the amount of users left right now.
        /// </summary>
        /// <returns></returns>
        private string AmountOfUsersLeft()
        {
            return Server.userList.Count().ToString();
        }
    }
}
