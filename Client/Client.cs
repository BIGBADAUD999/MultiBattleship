using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using ServerData;

namespace Client
{
    
    public partial class Client : Form
    {
        private int amountShips = 17;
        private char who;
        private bool myTurn=false;
        private ArrayList fields = new ArrayList();
        private bool isVertical = true;
        private Color dflt = Color.Navy;
        private Color mine = Color.LawnGreen;
        private Color ship = Color.DarkGreen;
        private Color border = Color.DarkGray;
        private Color dmg = Color.DarkRed;
        private Color miss = Color.Aquamarine;
        

        public Client()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            Point location1 = new Point(40, 90);
            Point location2 = new Point(375, 90);
            InitializeBoard(location1, 'A');
            InitializeBoard(location2, 'B');
            ConnectBtn.Click+=new EventHandler(ConnectBtn_Click);
            SendBtn.Click+=new EventHandler(SendBtn_Click);
            serverIP.Text = Packet.GetIP4Adress().ToString();
        }

        public void InitializeBoard(Point location1, char id1)
        {

            Point loc1 = location1;
            
            Size size = new Size(30, 30);

            loc1.Y -= 31;
            for (int i = 1; i < 10; i++)
            {
                Label labell = new Label();
                labell.AutoSize = false;
                labell.TextAlign = ContentAlignment.MiddleCenter;
                labell.Location = loc1;
                labell.Size = size;
                labell.MaximumSize = size;
                labell.Margin.All.Equals(1);
                labell.Text = i.ToString();
                labell.ForeColor = Color.Gold;
                labell.BackColor = Color.Transparent;
                this.Controls.Add(labell);
                loc1.X += 31;
            }

            loc1.X = location1.X - 31;
            loc1.Y = location1.Y;


            for (int i = 1; i < 10; i++)
            {
                Label labell = new Label();
                labell.AutoSize = false;
                labell.TextAlign = ContentAlignment.MiddleCenter;
                labell.Location = loc1;
                labell.Size = size;
                labell.MaximumSize = size;
                labell.Margin.All.Equals(1);
                labell.Text = toLetter(i).ToString();
                labell.ForeColor = Color.Gold;
                labell.BackColor = Color.Transparent;
                this.Controls.Add(labell);
                loc1.Y += 31;
            }

            loc1.X = location1.X;
            loc1.Y = location1.Y;

            
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    Button b = new Button();
                    b.Text = id1.ToString() + "_" + j.ToString() + "_" + i.ToString();
                    b.BackColor = dflt;
                    b.ForeColor = dflt;
                    b.Font = new Font("Arial", 1, FontStyle.Regular);
                    b.Margin.All.Equals(1);
                    b.MaximumSize = size;
                    b.AutoSize = false;
                    b.Size = size;
                    b.Location = loc1;
                    fields.Add(b);
                    this.Controls.Add(b);
                    loc1.X += 31;
                }
                loc1.Y += 31;
                loc1.X = location1.X;
            }
        }
        public char toLetter(int x)
        {
            if (x==0) return '?';
            if (x == 1) return 'A';
            if (x == 2) return 'B';
            if (x == 3) return 'C';
            if (x == 4) return 'D';
            if (x == 5) return 'E';
            if (x == 6) return 'F';
            if (x == 7) return 'G';
            if (x == 8) return 'H';
            if (x == 9) return 'I';
            else return '?';
        }

        public void button_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b.BackColor != ship)
            {
                if (amountShips > 0)
                {
                    b.BackColor = ship;
                    b.ForeColor = ship;
                    if (amountShips == 13 || amountShips == 9 || amountShips == 6 || amountShips == 3)
                    {
                        deployType.Enabled = true;
                    }
                    else
                    {
                        deployType.Enabled = false;
                    }
                    amountShips--;
                    int tempx = int.Parse(b.Text[2].ToString());
                    int tempy = int.Parse(b.Text[4].ToString());       
                    
                    Enable(tempx, tempy);
                    
                }
                if (amountShips == 0)
                {
                    btnRdy.Enabled = true;
                    btnRdy.Visible = true;
                    foreach (Button btn in fields)
                    {
                        if (btn.BackColor!=ship)
                        {
                            btn.BackColor = dflt;
                        }
                    }
                }
            }
            else
            {
                foreach (Button btn in fields)
                {
                    if (who == btn.Text[0])
                    {
                        btn.Click -= new EventHandler(button_Click);
                    }
                }
            }


        }

        public void button_Click2(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b.BackColor == dflt && myTurn==true)
            {
                myTurn = false;
                Packet p = new Packet(PacketType.Battleship_shot, ID, who);
                p.data.Add(b.Text);
                SendToServer(p);
            }
        }

        private void MsgBoard_TextChanged(object sender, EventArgs e)
        {
            MsgBoard.SelectionStart = MsgBoard.Text.Length;
            MsgBoard.ScrollToCaret();
        }

        private void btnRdy_Click(object sender, EventArgs e)
        {
            btnRdy.Enabled = false;
            btnRdy.Visible = false;
            btnRedeploy.Enabled = false;
            btnRedeploy.Visible = false;
            labelRdy.Text = "Wait..";
            Packet p = new Packet(PacketType.Battleship_set, ID, who);
            foreach (Button btn in fields)
            {
                if (btn.BackColor == ship)
                {
                    p.data.Add(btn.Text);
                }
                if (btn.Text[0] != who)
                {
                    btn.Click += new EventHandler(button_Click2);
                }
            }
            SendToServer(p);
        }

        private void Enable(int x, int y)
        {
            foreach (Button btn in fields)
            {
                if (who == btn.Text[0])
                {
                    btn.Enabled = false;
                    if (amountShips == 12 || amountShips == 8 || amountShips == 5 || amountShips == 2)
                    {
                        btn.Enabled = true;
                        foreach (Button b in fields)
                        {
                            int nx = int.Parse(b.Text[2].ToString());
                            int ny = int.Parse(b.Text[4].ToString());
                            if (b.BackColor == ship)
                            {

                                if (
                                   (int.Parse(btn.Text[2].ToString()) == nx && int.Parse(btn.Text[4].ToString()) == ny + 1)
                                   || (int.Parse(btn.Text[2].ToString()) == nx && int.Parse(btn.Text[4].ToString()) == ny - 1)
                                   || (int.Parse(btn.Text[2].ToString()) == nx + 1 && int.Parse(btn.Text[4].ToString()) == ny)
                                   || (int.Parse(btn.Text[2].ToString()) == nx - 1 && int.Parse(btn.Text[4].ToString()) == ny)
                                   || (int.Parse(btn.Text[2].ToString()) == nx - 1 && int.Parse(btn.Text[4].ToString()) == ny - 1)
                                   || (int.Parse(btn.Text[2].ToString()) == nx + 1 && int.Parse(btn.Text[4].ToString()) == ny + 1)
                                   || (int.Parse(btn.Text[2].ToString()) == nx - 1 && int.Parse(btn.Text[4].ToString()) == ny + 1)
                                   || (int.Parse(btn.Text[2].ToString()) == nx + 1 && int.Parse(btn.Text[4].ToString()) == ny - 1)
                                   )
                                {
                                    btn.Enabled = false;
                                    if (btn.BackColor==mine)
                                    {
                                        btn.BackColor = border;
                                    }
                                }
                            }
                        }

                    }
                    else if (isVertical == true)
                    {
                        if (
                            (int.Parse(btn.Text[2].ToString()) == x && int.Parse(btn.Text[4].ToString()) == y + 1)
                            || (int.Parse(btn.Text[2].ToString()) == x && int.Parse(btn.Text[4].ToString()) == y - 1)
                            )
                        {
                            if (btn.BackColor == mine)
                            {
                                btn.Enabled = true;
                            }
                        }
                    }
                    else if (isVertical == false)
                    {
                        if (
                            (int.Parse(btn.Text[2].ToString()) == x + 1 && int.Parse(btn.Text[4].ToString()) == y)
                            || (int.Parse(btn.Text[2].ToString()) == x - 1 && int.Parse(btn.Text[4].ToString()) == y)
                            )
                        {
                            if (btn.BackColor == mine)
                            {
                                btn.Enabled = true;
                            }
                        }
                    }

                }
            }
        }

        private void deployType_Click(object sender, EventArgs e)
        {
            if (isVertical == false)
            {
                isVertical = true;
                deployType.Text = "Deploying Veritcal..";
            }
            else if (isVertical==true)
            {
                isVertical = false;
                deployType.Text = "Deploying Horizontal..";
            }
        }

        private void btnRedeploy_Click(object sender, EventArgs e)
        {
            if (deployType.Enabled==false)
            {
                deployType.Enabled = true;
            }
            if (btnRdy.Visible==true)
            {
                btnRdy.Visible = false;
            }
            foreach (Button btn in fields)
            {
                if (who==btn.Text[0])
                {
                    btn.Enabled = true;
                    btn.BackColor = mine;
                    btn.ForeColor = mine;
                    amountShips = 17;
                }
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            //brute exit, had to 
            destroy();       
        }
    }
}
