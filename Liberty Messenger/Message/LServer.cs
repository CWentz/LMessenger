using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;
using System.Collections.Generic;

namespace LMessengerCore 
{
    public class LServer
    {
        private Thread mainThread;
        private static Hashtable userNames;
        private static Hashtable userNickNames;
        private static Hashtable userPassword;
        private int maxConnections;
        private TcpListener listener;
        private bool stayConnected;



        /// <summary>
        /// establishes the server settings and starts the server.
        /// </summary>
        /// <param name="portNumber">port to connect with</param>
        /// <param name="userLimit">max amount of users</param>
        public LServer(IPAddress ip, int port, int userLimit)
        {
            maxConnections = userLimit;
            userNames = new Hashtable(userLimit);
            userNickNames = new Hashtable(userLimit);
            userPassword = new Hashtable(userLimit);
            listener = new TcpListener(ip, port);
            
        }

        /// <summary>
        /// this is where the server loop is handled.
        /// </summary>
        private void ServerLoop()
        {
            int debug = 0;

            while(this.stayConnected)
            {
                if (this.listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected");

                    LUser user = new LUser(client);
                    //this.userNameList.Add(user.UserName);
                }

                //used as a failsafe
                debug++;
                if(debug == int.MaxValue)
                {
                    this.stayConnected = false;
                }
            }

            //if loop exits then shut server down.
            this.StopServer();
        }

        /// <summary>
        /// starts the server and continues to server loop.
        /// </summary>
        public void StartServer()
        {
            if (this.stayConnected) return;

            this.listener.Start();

            this.stayConnected = true;

            this.mainThread = new Thread(new ThreadStart(ServerLoop));
            this.mainThread.Start();
        }

        /// <summary>
        /// stops the server.
        /// </summary>
        public void StopServer()
        {
            if (!this.stayConnected) return;

            this.stayConnected = false;
            TcpClient[] tcpClients = new TcpClient[userNickNames.Count];
            userNickNames.Values.CopyTo(tcpClients, 0);

            SendSystemMessage("Server Shutdown");

            for (int client = 0; client < tcpClients.Length; client++)
            {
                if (tcpClients[client] == null) continue;

                tcpClients[client].Close();
            }
            this.listener.Stop();
            this.mainThread.Abort();
        }

        /// <summary>
        /// sends a message out to all connected users.
        /// </summary>
        /// <param name="user">user name</param>
        /// <param name="message">message text</param>
        public static void SendGlobalMessage(string user, string message)
        {
            StreamWriter writer;
            //ArrayList remove

            TcpClient[] tcpClients = new TcpClient[userNickNames.Count];
            userNickNames.Values.CopyTo(tcpClients, 0);

            for(int client = 0; client < tcpClients.Length; client++)
            {
                if(tcpClients[client] == null || message == "") continue;

                try
                {
                    writer = new StreamWriter(tcpClients[client].GetStream());
                    writer.WriteLine(user + ": " + message);
                    writer.Flush();
                    writer = null;
                }
                catch (Exception error)
                {
                    Console.WriteLine("Error: " + error);
                    string str = (string)userNames[tcpClients[client]];
                    SendSystemMessage("**** " + str + " **** has left the room.");
                    userNickNames.Remove(str);
                    userNames.Remove(str);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void SendSystemMessage(string message)
        {
            StreamWriter writer;

            TcpClient[] tcpClients = new TcpClient[userNickNames.Count];
            userNickNames.Values.CopyTo(tcpClients, 0);

            for(int client = 0; client < tcpClients.Length; client++)
            {
                if(tcpClients[client] == null || message == "") continue;

                try
                {
                    writer = new StreamWriter(tcpClients[client].GetStream());
                    writer.WriteLine(message);
                    writer.Flush();
                    writer = null;
                }
                catch (Exception error)
                {
                    Console.WriteLine("Error: " + error);
                    userNickNames.Remove(userNames[tcpClients[client]]);
                    userNames.Remove(userNames[tcpClients[client]]);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="nickname"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ELoginError ConnectAsUser(LUser user)
        {
            //test if user even exists first
            if (userNames.ContainsKey(user.UserName))
            {
                //test the password with the username to see if it is that user
                if (userPassword[user.UserName] != user.Password)
                    return ELoginError.BadPass;

                //test if the nickname is already in use by another user
                if (userNickNames.ContainsKey(user.NickName))
                {
                    //if the assigned nickname isn't for the user then returns in use.
                    if ((string)userNickNames[user.NickName] != user.UserName)
                    {
                        return ELoginError.NicknameInUse;
                    }
                    else
                    {
                        //the username was assigned to the user return no errors.
                        return ELoginError.None;
                    }
                }

                //no nickname found for that user so bind/add the username and nickname.
                //userNames.Add(user.UserName, user.Client);
                userNickNames.Add(user.NickName, user.UserName);
                return ELoginError.None;
            }
            
            //create user from scratch since no user name was found
            userNames.Add(user.UserName, user.Client);
            userNickNames.Add(user.NickName, user.UserName);
            userPassword.Add(user.UserName, user.Password);

            //all good in the hood
            return ELoginError.None;
        }
    }
}
