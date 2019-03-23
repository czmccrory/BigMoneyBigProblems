using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace ThreadATM
{
    public partial class ATM : Form
    {
        private Account[] ac = new Account[3];

        //textBox
        TextBox Box = new TextBox();

        public ATM()
        {
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);

            InitializeComponent();
            StartScreen();
        }

        public void StartScreen()
        {
            //adds gif
            PictureBox BankGif = new PictureBox();
            BankGif.SizeMode = PictureBoxSizeMode.StretchImage;
            BankGif.Size = new Size(350, 200);
            BankGif.Location = new Point(0, 0);
            BankGif.Image = Image.FromFile("BOA.GIF");
            Controls.Add(BankGif);

            //text
            Label askUser = new Label();
            askUser.AutoSize = true;
            askUser.Location = new Point(80,200);
            askUser.ForeColor = Color.DarkBlue;
            askUser.Font = new Font("Franklin Gothic", 8);
            askUser.Text = "Please enter your account number";
            Controls.Add(askUser);

            //textbox -had to initialise out method for the event handler, try and fix?           
            Box.Location = new Point(100, 230);
            Box.TextAlign = HorizontalAlignment.Center;
            Box.Height = 40;
            Box.Width = 130;
            Controls.Add(Box);

            //KeyPad
            Button [,] grid = new Button[3, 4];
            int num = 1;
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    //Setting the characteristics for the buttons in the grid                
                    grid[x, y] = new Button();
                    grid[x, y].SetBounds((50 * x) + 95, (50 * y) + 220 + 40, 40, 40);
                    grid[x, y].BackColor = Color.LightGray;
                    grid[x, y].TabStop = false;
                    grid[x, y].FlatStyle = FlatStyle.Flat;
                    grid[x, y].FlatAppearance.BorderSize = 1;
                    grid[x, y].Text = "" + num;
                    grid[x, y].MouseDown += new MouseEventHandler(this.gridEvent_MouseDown);
                    num++;
                    Controls.Add(grid[x, y]);
                }
            }
            grid[0, 3].Text = "Clear";
            grid[0, 3].BackColor = Color.Yellow;
            grid[1, 3].Text = "0";
            grid[2, 3].Text = "Enter";
            grid[2, 3].BackColor = Color.Green;

            
        }

        private void gridEvent_MouseDown(object sender, MouseEventArgs e)
        {
            if (((Button)sender).Text == "Clear") {Box.Text = String.Empty;}

            else if (((Button)sender).Text == "Enter")
            {
                if (findAccount(int.Parse(Box.Text)))
                {
                    Box.Text ="correct";//--------------------------------------------------------------------
                }
                else
                {
                    Box.Text = String.Empty;
                }
            }

            else { Box.Text += ((Button)sender).Text; }            
        }

        private bool findAccount(int input)
        {

            for (int i = 0; i < this.ac.Length; i++)
            {
                if (ac[i].getAccountNum() == input)
                {
                    return true;
                }
            }

            return false;
        }
    }

    class Account
    {
        //the attributes for the account
        private int balance;
        private int pin;
        private int accountNum;

        // a constructor that takes initial values for each of the attributes (balance, pin, accountNumber)
        public Account(int balance, int pin, int accountNum)
        {
            this.balance = balance;
            this.pin = pin;
            this.accountNum = accountNum;
        }

        //getter and setter functions for balance
        public int getBalance()
        {
            return balance;
        }
        public void setBalance(int newBalance)
        {
            this.balance = newBalance;
        }

        /*
         *   This funciton allows us to decrement the balance of an account
         *   it perfomes a simple check to ensure the balance is greater tha
         *   the amount being debeted
         *   
         *   reurns:
         *   true if the transactions if possible
         *   false if there are insufficent funds in the account
         */
        public Boolean decrementBalance(int amount)
        {
            if (this.balance > amount)
            {
                balance -= amount;
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
         * This funciton check the account pin against the argument passed to it
         *
         * returns:
         * true if they match
         * false if they do not
         */
        public Boolean checkPin(int pinEntered)
        {
            if (pinEntered == pin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int getAccountNum()
        {
            return accountNum;
        }

    }
}
