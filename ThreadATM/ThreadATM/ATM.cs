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

        private Account activeAccount = null;

        int currentAccountNumber;
        bool AccountFound = false;

        static Thread ATM1 = new Thread(new ThreadStart(CreateAndShowForm));
        static Thread ATM2 = new Thread(new ThreadStart(CreateAndShowForm));

        //initial screen objects
        TextBox Box = new TextBox();
        Label askUser = new Label();
        PictureBox BankGif = new PictureBox();
        Button[,] grid = new Button[3, 4];

        //options screen
        Button[] optionButton = new Button[3];

        //withdraw screen objects
        Button[,] withdrawOptions = new Button[2, 3];

        public ATM()
        {
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
            CentralComp.setupCentralComp();
            InitializeComponent();
            StartScreen();
        }

        private void ThreadProc()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => CreateAndShowForm()));
                return;
            }

            CreateAndShowForm();
        }

        public static void CreateAndShowForm()
        {
            var frm = new ATM();
            frm.ShowDialog();
        }

        public void StartScreen()
        {
            //adds gif
            BankGif.SizeMode = PictureBoxSizeMode.StretchImage;
            BankGif.Size = new Size(350, 200);
            BankGif.Location = new Point(0, 0);
            BankGif.Image = Image.FromFile("BOA.GIF");
            Controls.Add(BankGif);

            //text
            askUser.AutoSize = true;
            askUser.Location = new Point(80, 200);
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

            keypad();
        }

        public void keypad()
        {
            //KeyPad
            int num = 1;
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    //Setting the characteristics for the buttons in the grid                
                    grid[x, y] = new Button();
                    grid[x, y].SetBounds((55 * x) + 85, (55 * y) + 220 + 40, 50, 50);
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
            if (((Button)sender).Text == "Clear") { Box.Text = String.Empty; }

            else if (((Button)sender).Text == "Enter")
            {
                if (AccountFound == false)
                {
                    switch(Box.Text)
                    {
                        case "":
                            break;
                        default:
                            activeAccount = CentralComp.getAccount(int.Parse(Box.Text));//findAccount(int.Parse(Box.Text));
                            break;
                    }
                    
                    Box.Text = String.Empty;

                    if (activeAccount != null)
                    {
                        currentAccountNumber = activeAccount.getAccountNum();
                        AccountFound = true;
                        Box.Text = string.Empty;
                        askUser.Text = "Please enter your pin number";
                    }
                    else
                    {
                        askUser.Text = "Incorrect account number, please try again";
                        Box.Text = String.Empty;
                    }
                }

                else
                {
                    if (activeAccount.checkPin(int.Parse(Box.Text)))
                    {
                        Box.Text = String.Empty;
                        AccountFound = false;
                        options();
                    }
                    else
                    {
                        askUser.Text = "Incorrect pin number, please try again";
                        Box.Text = String.Empty;
                    }
                }
            }

            else { Box.Text += ((Button)sender).Text; }
        }

        private Account findAccount(int input)
        {

            for (int i = 0; i < this.ac.Length; i++)
            {
                if (ac[i].getAccountNum() == input)
                {
                    return ac[i];
                }
            }

            return null;
        }

        public void options()
        {
            Controls.Clear();
            keypad();

            int vertical = 30;
            for (int x = 0; x < optionButton.GetLength(0); x++)
            {
                optionButton[x] = new Button();
                optionButton[x].Location = new Point(105, vertical);
                optionButton[x].Size = new Size(120, 50);
                optionButton[x].BackColor = Color.LightGray;
                optionButton[x].FlatStyle = FlatStyle.Flat;
                switch (x)
                {
                    case 0:
                        optionButton[0].Text = "Withdraw";
                        optionButton[x].Click += new EventHandler(Withdraw_Click);
                        break;
                    case 1:
                        optionButton[1].Text = "Balance";
                        optionButton[x].Click += new EventHandler(Balance_Click);
                        break;
                    case 2:
                        optionButton[2].Text = "Cancel";
                        optionButton[x].Click += new EventHandler(Cancel_Click);
                        break;
                }
                Controls.Add(optionButton[x]);
                vertical += 80;
            }
        }

        private void Withdraw_Click(object sender, EventArgs e)
        {
            withdrawScreen();
        }

        private void Balance_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            keypad();

            Label display = new Label();
            display.Location = new Point(110, 100);
            display.Font = new Font("Ariel", 10);
            display.Text = "Balance: £" + (showBalance()).ToString();
            Controls.Add(display);

            Button goBack = new Button();
            goBack.Text = "Go Back";
            goBack.Location = new Point(105, 140);
            goBack.Size = new Size(120, 50);
            goBack.BackColor = Color.LightGray;
            goBack.FlatStyle = FlatStyle.Flat;
            goBack.Click += new EventHandler(goBack_Click);
            Controls.Add(goBack);
        }

        private void goBack_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            keypad();
            options();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            StartScreen();
        }

        private int showBalance()
        {
            return activeAccount.getBalance();
        }

        public void withdrawScreen()
        {
            Controls.Clear();
            keypad();
            for (int x = 0; x < withdrawOptions.GetLength(0); x++)
            {
                for (int y = 0; y < withdrawOptions.GetLength(1); y++)
                {
                    //Setting the characteristics for the buttons in the grid                
                    withdrawOptions[x, y] = new Button();
                    withdrawOptions[x, y].SetBounds((220 * x) + 5, (80 * y) + 5 + 40, 100, 50);
                    withdrawOptions[x, y].BackColor = Color.LightGray;
                    withdrawOptions[x, y].TabStop = false;
                    withdrawOptions[x, y].FlatStyle = FlatStyle.Flat;
                    withdrawOptions[x, y].FlatAppearance.BorderSize = 1;
                    switch (y)
                    {
                        case 0:
                            withdrawOptions[x, y].Text = "£5";
                            withdrawOptions[x, y].Name = "5";
                            if (x == 1) { withdrawOptions[x, y].Text = "£40"; withdrawOptions[x, y].Name = "40"; }
                            break;
                        case 1:
                            withdrawOptions[x, y].Text = "£10";
                            withdrawOptions[x, y].Name = "10";
                            if (x == 1) { withdrawOptions[x, y].Text = "£100"; withdrawOptions[x, y].Name = "100"; }
                            break;
                        case 2:
                            withdrawOptions[x, y].Text = "£20";
                            withdrawOptions[x, y].Name = "20";
                            if (x == 1) { withdrawOptions[x, y].Text = "£500"; withdrawOptions[x, y].Name = "500"; }
                            break;
                    }
                    withdrawOptions[x, y].MouseDown += new MouseEventHandler(this.withdrawOptionsEvent_MouseDown);
                    Controls.Add(withdrawOptions[x, y]);
                }
            }
        }

        private void withdrawOptionsEvent_MouseDown(object sender, MouseEventArgs e)
        {
            if (activeAccount.getBalance() > int.Parse(((Button)sender).Name))
            {
                int subtraction = activeAccount.getBalance() - int.Parse(((Button)sender).Name);
                activeAccount.setBalance(subtraction);
                options();
            }
            else
            {
                options();
            }
        }

        private void ATM_Load(object sender, EventArgs e)
        {

        }

        static void Main()
        {
            ATM2.Start();
            Application.Run(new ATM());
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

    class CentralComp
    {

        public static Account[] ac = new Account[3];
        
        //public CentralComp()
        //{
        //    //ac[0] = new Account(300, 1111, 111111);
        //    //ac[1] = new Account(750, 2222, 222222);
        //    //ac[2] = new Account(3000, 3333, 333333);
        //}

        public static void setupCentralComp()
        {
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
        }

        public static Account getAccount(int acNum)
        {
            for (int i = 0; i < ac.Length; i++)
            {
                if (ac[i].getAccountNum() == acNum)
                {
                    return ac[i];
                }
            }
            return null;
        }

        public static void updateBalance(Account accToUpdate, int newBalance)
        {
            for (int i = 0; i < ac.Length; i++)
            {
                if (ac[i] == accToUpdate)
                {
                    ac[i].setBalance(newBalance);
                    return;
                }
            }
        }

    }

}