using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerData;

namespace Server
{
    class ClientData
    {
        public static volatile char who = 'A';
        public static volatile int x = 0;
        public Socket clientSocket;
        public Thread clientThread;
        public string id;

        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();

            clientThread = new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            sendRegistrationPacketToClient();
        }

        public void sendRegistrationPacketToClient()
        {
            if (x < 2)
            {
                Packet p = new Packet(PacketType.Registration, "server", who);
                x++;
                who = 'B';
                p.data.Add(id);
                clientSocket.Send(p.ToBytes());
            }
        }
    }
}
