using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using ServerData;

namespace Client
{

    public partial class Client
    {
        public static Socket socket;
        public static IPAddress ipAdress;
        public static string ID;
        public static string login;
        public static Thread thread;
        public static bool isConnected = false;
        private bool dstrmsgchk = false;

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Login.Text)) //login validation
            {
                AppendMsgBoard("Wrong ID \r\n");
            }
            else if (!IPAddress.TryParse(serverIP.Text, out ipAdress)) //ip validation
            {
                AppendMsgBoard("Wrong IP address\r\n");
            }
            else //connection to server
            {
                Login.ReadOnly = true;
                Login.Enabled = false;
                serverIP.ReadOnly = true;
                serverIP.Enabled = false;
                ConnectBtn.Visible = false;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAdress, 4242);

                dstrmsgchk = true;

                try
                {
                    socket.Connect(ipEndPoint);
                    login = Login.Text;

                    isConnected = true;
                    ConnectBtn.Enabled = false;
                    SendBtn.Enabled = true;

                    deployType.Enabled = true;
                    deployType.Visible = true;

                    btnRedeploy.Visible = true;

                    btnRdy.Enabled = true;
   
                    thread = new Thread(Data_IN);
                    thread.Start();
                }
                catch (SocketException ex)
                {
                    AppendMsgBoard("Error during connecting to server..\r\n");
                    dstrmsgchk = false;
                }
            }
        }

        private void Data_IN()
        {
            byte[] buffer;
            int readBytes;
            AppendMsgBoard("Waiting for incomming data..\r\n");
            while (true)
            {
                try
                {
                    buffer = new byte[socket.SendBufferSize];
                    readBytes = socket.Receive(buffer);
                    
                    if (readBytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }
                }
                catch (SocketException ex)
                {
                    AppendMsgBoard("Error during receiving data..\r\n");
                }

            }
        }

        private void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Registration:
                    ID = p.data[0];
                    who = p.senderWho;
                    Packet packet = new Packet(PacketType.Chat, ID);
                    packet.data.Add(login);
                    packet.data.Add("Connected");
                    socket.Send(packet.ToBytes());
                    SetLabelVisibility();
                    break;


                case PacketType.Chat:
                case PacketType.CloseConnection:
                    if (p.data.Count == 2)
                    {
                        AppendMsgBoard(p.data[0] + ": " + p.data[1] + "\r\n");
                    }
                    else
                    {
                        foreach (string str in p.data)
                        {
                            AppendMsgBoard(str + "\r\n");
                        }
                    }
                    if (p.packetBool == true )
                    {
                        if (who == 'A')
                        {
                            myTurn = true;
                            SetLabelRdy("Your Turn");
                        }
                        else if (who == 'B')
                        {
                            SetLabelRdy("Opponet's Turn");
                        }
                    }
                    break;
                case PacketType.Battleship_shot:
                    Shoot(p);
                    break;
                case PacketType.Game_over:
                    GameOver(p);
                    break;
            }
        }

        private void GameOver(Packet p)
        {
            myTurn = false;
            AppendMsgBoard(p.data[0]+"\r\n");
            SetLabelVisibility2();
        }

        private void SetLabelVisibility2()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(SetLabelVisibility2), new object[] { });
                return;
            }
            labelRdy.Visible = false;
        }
        private void Shoot(Packet p)
        {
            bool h = false;
            if (p.data.Count==2 && p.senderWho == who)  
            {
                myTurn = true;
            }
            else if (p.data.Count==2 && p.senderWho != who)  
            {
                myTurn = false;
            }
            else if (p.senderWho == who)                
            {
                myTurn = false;
            }
            else if (p.senderWho != who)                
            {
                myTurn = true;
            }
            foreach (Button b in fields)
            {
                if (b.Text == p.data[0])
                {
                    if (p.data.Count == 2)
                    {
                        b.BackColor = dmg;
                        b.ForeColor = dmg;
                    }
                    else
                    {
                        b.BackColor = miss;
                        b.ForeColor = miss;
                    }
                }
            }
            try
            {
                AppendMsgBoard(p.data[1] + "\r\n");
            }
            catch (Exception e)
            {
                //
            }
            
            if (myTurn == true)
            {
                SetLabelRdy("Your Turn");
            }
            else if (myTurn == false)
            {
                SetLabelRdy("Opponet's Turn");
            }
        }

        private void SetLabelRdy(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(SetLabelRdy), new object[] { str });
                return;
            }
            else
            {
                labelRdy.Text = str;
            }
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {   
            string msg = Msg.Text;
            Msg.Text = string.Empty;

            Packet p = new Packet(PacketType.Chat, ID);
            p.data.Add(login);
            p.data.Add(msg);
            SendToServer(p);
        }

        private void SendToServer(Packet p)
        {
            try
            {
                socket.Send(p.ToBytes());
            }
            catch (Exception ex)
            {
                AppendMsgBoard("Couldn't send this msg\r\n");
                AppendMsgBoard(ex.ToString()+"\r\n");
                Msg.Text = "";
            }
        }

        private void AppendMsgBoard(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendMsgBoard), new object[] { value });
                return;
            }
            MsgBoard.Text += value;
        }
        
        private void SetLabelVisibility()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(SetLabelVisibility), new object[] { });
                return;
            }
            else
            {
                if (who == 'A')
                {
                    labelWho.Text = "Player A: " + login;
                    labelWho.BackColor = Color.Transparent;
                    foreach (Button b in fields)
                    {
                        if (b.Text[0] == 'A')
                        {
                            b.Click += new EventHandler(button_Click);
                        }
                    }
                }
                else if (who == 'B')
                {
                    labelWho.Text = "Player B: " + login;
                    labelWho.BackColor = Color.Transparent;
                    foreach (Button b in fields)
                    {
                        //AppendMsgBoard(label.Text[0].ToString());
                        if (b.Text[0] == 'B')
                        {
                            b.Click += new EventHandler(button_Click);
                        }
                    }
                }
                labelRdy.Visible = true;
                foreach (Button b in fields)
                {
                    if (b.Text[0] == who)
                    {
                        b.BackColor = mine;
                        b.ForeColor = mine;
                    }
                }
            }
        }

        private void destroy()
        {
            if (dstrmsgchk==true)
            {
                Packet packet = new Packet(PacketType.Chat, ID);
                packet.data.Add(login);
                packet.data.Add("Disconnected");
                socket.Send(packet.ToBytes());
                dstrmsgchk = false;
            }
            this.Close();
            this.Dispose();
        }
    }
}