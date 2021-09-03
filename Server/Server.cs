using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerData;

namespace Server
{
    class Server
    {
        private static int A = 17;
        private static int B = 17;
        public static ArrayList ShipsA = new ArrayList();
        public static ArrayList ShipsB = new ArrayList();
        static Socket listenerSocket;
        static List<ClientData> _clients;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server...");
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Adress()), 4242);
            listenerSocket.Bind(ip);

            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();

            Console.WriteLine("Success... Listening IP: " + Packet.GetIP4Adress() + "\n port: 4242");
        }

        static void ListenThread()  
        {
            while (true)
            {
                listenerSocket.Listen(0);
                _clients.Add(new ClientData(listenerSocket.Accept()));
            }
        }

        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] Buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        Packet p = new Packet(Buffer);
                        DataManager(p);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Client Disconnected.");

                    //Console.WriteLine(ex);
                    break;
                }
            }
        }

        public static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Chat:
                    SendMessageToClients(p);
                    break;


                case PacketType.CloseConnection:
                    var exitClient = GetClientByID(p);
                    Console.WriteLine("Client Disconnected.");
                    SendMessageToClients(p);
                    CloseClientConnection(exitClient);
                    RemoveClientFromList(exitClient);
                    AbortClientThread(exitClient);

                    break;
                case PacketType.Battleship_set:
                    if (p.senderWho == 'A')
                    {
                        foreach (string str in p.data)
                        {
                            ShipsA.Add(new Field(str));
                        }
                    }
                    else if (p.senderWho == 'B')
                    {
                        foreach(string str in p.data)
                        {
                            ShipsB.Add(new Field(str));
                        }
                    }
                    if (ShipsA.Count == 17 && ShipsB.Count == 17)
                    {
                        StartBattle();
                    }
                    break;

                case PacketType.Battleship_shot:
                    CheckShot(p);
                    break;
            }
        }
        
        public static void CheckShot(Packet p)
        {
            bool f = false;
            if (p.senderWho == 'A')
            {
                foreach (Field field in ShipsB)
                {
                    if (p.data[0] == field.str)
                    {
                        B--;
                        GoodShot('A', field.str);
                        f = true;
                    }
                }
                if (f != true)
                {
                    BadShot('A', p.data[0]);
                }
            }
            else if (p.senderWho == 'B')
            {
                foreach (Field field in ShipsA)
                {
                    if (p.data[0] == field.str)
                    {
                        A--;
                        GoodShot('B', field.str);
                        f = true;
                    }
                }
                if (f != true)
                {
                    BadShot('B', p.data[0]);
                }
            }
        }

        public static void GoodShot(char _who, string _field)
        {
            Packet p = new Packet(PacketType.Battleship_shot, "Server");
            p.senderWho = _who;
            p.data.Add(_field);
            if (_who == 'A')
            {
                p.data.Add("Player " + _who.ToString() + " hits\r\nPlayer B have: " + B.ToString() + " health left");
            }
            else
            {
                p.data.Add("Player " + _who.ToString() + " hits\r\nPlayer A have: " + A.ToString() + "health left");
            }
            p.packetBool = true;
            SendMessageToClients(p);
            if (A == 0)
            {
                Packet pa = new Packet(PacketType.Game_over, "Server");
                pa.data.Add("Game over, Player B wins!");
                SendMessageToClients(pa);
            }
            else if (B == 0)
            {
                Packet pa = new Packet(PacketType.Game_over, "Server");
                pa.data.Add("Game over, Player A wins!");
                SendMessageToClients(pa);
            }
        }

        public static void BadShot(char _who, string _field)// miss on chat?
        {
            Packet p = new Packet(PacketType.Battleship_shot, "Server");
            p.senderWho = _who;
            p.data.Add(_field);
            p.packetBool = false;
            SendMessageToClients(p);
        }
        public static void StartBattle()
        {
            Packet pa = new Packet(PacketType.Chat, "Server");
            pa.data.Add("Both players positioned their ships");
            pa.packetBool = true;
            SendMessageToClients(pa);
        }

        public static void SendMessageToClients(Packet p)
        {
            foreach (ClientData c in _clients)
            {
                c.clientSocket.Send(p.ToBytes());
            }
        }

        private static ClientData GetClientByID(Packet p)
        {
            return (from client in _clients
                    where client.id == p.senderID
                    select client)
                    .FirstOrDefault();
        }

        private static void CloseClientConnection(ClientData c)
        {
            c.clientSocket.Close();
        }
        private static void RemoveClientFromList(ClientData c)
        {
            _clients.Remove(c);
        }
        private static void AbortClientThread(ClientData c)
        {
            c.clientThread.Abort();
           
        }


        

        }

    }

