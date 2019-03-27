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
using System.Timers;

namespace ThreadATM
{
    
    public partial class ATM : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        int yfinal = 450;

        private Account activeAccount = null;

        int currentAccountNumber;
        bool AccountFound = false;
        bool showDataRace = false;
        bool otherClicked = false;
        bool depositClicked = false;
        bool changePinClicked = false;

        static Thread ATM1 = new Thread(new ThreadStart(CreateAndShowForm));
        static Thread ATM2 = new Thread(new ThreadStart(CreateAndShowForm));
        static Semaphore sem = new Semaphore(1, 1);

        //initial screen objects
        TextBox Box = new TextBox();
        Label askUser = new Label();
        PictureBox BankGif = new PictureBox();
        Button[,] grid = new Button[3, 4];
        CheckBox dataRaceCheck = new CheckBox();
        PictureBox cardGif = new PictureBox();
        int pinTried = 0;

        //options screen
        Button[,] optionButton = new Button[2,3];
        TextBox depositTextBox = new TextBox();

        //withdraw screen objects
        Button[,] withdrawOptions = new Button[2, 3];
        PictureBox bill = new PictureBox();
        TextBox otherTextBox = new TextBox();

        public ATM()
        {
            //ac[0] = new Account(300, 1111, 111111);
            //ac[1] = new Account(750, 2222, 222222);
            //ac[2] = new Account(3000, 3333, 333333);
            InitializeComponent();
            StartScreen();
        }

        //private void ThreadProc()
        //{
        //    if (InvokeRequired)
        //    {
        //        this.Invoke(new Action(() => CreateAndShowForm()));
        //        return;
        //    }

        //    CreateAndShowForm();
        //}

        public static void CreateAndShowForm()
        {
            var frm = new ATM();
            frm.ShowDialog();
        }

        public void StartScreen()
        {
            Controls.Clear();

            //textbox          
            Box.Location = new Point(160, 250);
            Box.TextAlign = HorizontalAlignment.Center;
            Box.Height = 100;
            Box.Width = 60;
            Box.BringToFront();
            Controls.Add(Box);

            //text
            askUser.AutoSize = true;
            askUser.Location = new Point(100, 230);
            askUser.ForeColor = Color.DarkBlue;
            askUser.Font = new Font("Franklin Gothic", 8);
            askUser.Text = "Please enter your account number";
            Controls.Add(askUser);

            //adds gif
            BankGif.SizeMode = PictureBoxSizeMode.StretchImage;
            BankGif.Size = new Size(272, 200);
            BankGif.Location = new Point(60, 80);
            BankGif.Image = Image.FromFile("BOA.GIF");
            BankGif.SendToBack();
            Controls.Add(BankGif);

            //entering card
            cardGif.SizeMode = PictureBoxSizeMode.StretchImage;
            cardGif.Size = new Size(60, 120);
            cardGif.Location = new Point(290, 370);
            cardGif.Image = Image.FromFile("card.GIF");
            Controls.Add(cardGif);

            //check box
            dataRaceCheck.Location = new Point(0,0);
            dataRaceCheck.Size = new Size(165, 20);
            dataRaceCheck.Text = "Show Data Race Condition?";
            Controls.Add(dataRaceCheck);

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
                    grid[x, y].SetBounds((48 * x) + 124, (45 * y) + 260 + 40, 50, 47);
                    grid[x, y].FlatStyle = FlatStyle.Flat;
                    grid[x, y].TabStop = false;
                    grid[x, y].Name = "" + num;
                    grid[x, y].MouseDown += new MouseEventHandler(this.gridEvent_MouseDown);
                    num++;
                    Controls.Add(grid[x, y]);
                }
            }
            grid[0, 0].BackgroundImage = Image.FromFile("1.jpg");
            grid[0, 0].BackgroundImageLayout = ImageLayout.Stretch;
            grid[1, 0].BackgroundImage = Image.FromFile("2.jpg");
            grid[1, 0].BackgroundImageLayout = ImageLayout.Stretch;
            grid[2, 0].BackgroundImage = Image.FromFile("3.jpg");
            grid[2, 0].BackgroundImageLayout = ImageLayout.Stretch;
            grid[0, 1].BackgroundImage = Image.FromFile("4.jpg");
            grid[0, 1].BackgroundImageLayout = ImageLayout.Stretch;
            grid[1, 1].BackgroundImage = Image.FromFile("5.jpg");
            grid[1, 1].BackgroundImageLayout = ImageLayout.Stretch;
            grid[2, 1].BackgroundImage = Image.FromFile("6.jpg");
            grid[2, 1].BackgroundImageLayout = ImageLayout.Stretch;
            grid[0, 2].BackgroundImage = Image.FromFile("7.jpg");
            grid[0, 2].BackgroundImageLayout = ImageLayout.Stretch;
            grid[1, 2].BackgroundImage = Image.FromFile("8.jpg");
            grid[1, 2].BackgroundImageLayout = ImageLayout.Stretch;
            grid[2, 2].BackgroundImage = Image.FromFile("9.jpg");
            grid[2, 2].BackgroundImageLayout = ImageLayout.Stretch;
            grid[0, 3].BackgroundImage = Image.FromFile("Clear.jpg");
            grid[0, 3].BackgroundImageLayout = ImageLayout.Stretch;
            grid[1, 3].BackgroundImage = Image.FromFile("0.jpg");
            grid[1, 3].BackgroundImageLayout = ImageLayout.Stretch;
            grid[2, 3].BackgroundImage = Image.FromFile("Enter.jpg");
            grid[2, 3].BackgroundImageLayout = ImageLayout.Stretch;
            grid[0, 3].Name = "Clear";
            grid[1, 3].Name = "0";
            grid[2, 3].Name = "Enter";
        }

        private void gridEvent_MouseDown(object sender, MouseEventArgs e)
        {
            if (otherClicked == false && depositClicked == false)
            {
                if (((Button)sender).Name == "Clear") { Box.Text = String.Empty; }

                else if (((Button)sender).Name == "Enter")
                {
                    showDataRace = dataRaceCheck.Checked;
                    if (AccountFound == false)
                    {
                        switch (Box.Text)
                        {
                            case "":
                                break;
                            default:
                                activeAccount = CentralComp.getAccount(int.Parse(Box.Text));
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
                        if (Box.Text == "")
                        {
                            pinTried++;
                            askUser.Text = "Incorrect pin number, you have " + (3 - pinTried) + " guesses left";
                            Box.Text = String.Empty;
                        }
                        else if (activeAccount.checkPin(int.Parse(Box.Text)))
                        {
                            Box.Text = String.Empty;
                            AccountFound = false;
                            options();
                        }
                        else
                        {
                            pinTried++;
                            askUser.Text = "Incorrect pin number, you have " + (3 - pinTried) + " guesses left";
                            Box.Text = String.Empty;
                        }
                        if (pinTried > 2)
                        {
                            pinTried = 0;
                            Box.Text = String.Empty;
                            AccountFound = false;
                            activeAccount = null;
                            StartScreen();
                            askUser.Text = "card declined, Enter account number";

                        }
                    }
                }

                else { Box.Text += ((Button)sender).Name; }
            }
            else if (otherClicked == true)
            {
                if (((Button)sender).Name == "Clear") { Box.Text = string.Empty; }

                else if (((Button)sender).Name == "Enter")
                {
                    if (!showDataRace)
                    {
                        sem.WaitOne();
                        activeAccount.setBalance(CentralComp.getBalance(activeAccount.getAccountNum()));
                    }
                    activeAccount.decrementBalance(int.Parse(Box.Text));
                    CentralComp.updateAccount(activeAccount.getAccountNum(), activeAccount.getBalance());
                    Controls.Clear();
                    Box.Text = String.Empty;

                    if (!showDataRace)
                    {
                        sem.Release();
                    }

                    options();
                }
                else { Box.Text += ((Button)sender).Name; }
            }
            else if (depositClicked == true)
            {
                if (((Button)sender).Name == "Clear") { Box.Text = string.Empty; }

                else if (((Button)sender).Name == "Enter")
                {
                    showDataRace = dataRaceCheck.Checked;
                    activeAccount.incrementBalance(int.Parse(Box.Text));
                    CentralComp.updateAccount(activeAccount.getAccountNum(), activeAccount.getBalance());
                    Controls.Clear();
                    Box.Text = String.Empty;
                    options();
                }
                else { Box.Text += ((Button)sender).Name; }
            }
            else if (changePinClicked == true)
            {
                showDataRace = dataRaceCheck.Checked;
                activeAccount.setPin(int.Parse(Box.Text));
                Controls.Clear();
                Box.Text = String.Empty;
                options();
            }
        }

        //private Account findAccount(int input)
        //{

        //    for (int i = 0; i < this.ac.Length; i++)
        //    {
        //        if (ac[i].getAccountNum() == input)
        //        {
        //            return ac[i];
        //        }
        //    }

        //    return null;
        //}

        public void options()
        {
            Controls.Clear();
            keypad();

            for (int x = 0; x < optionButton.GetLength(0); x++)
            {
                for (int y = 0; y < optionButton.GetLength(1); y++)
                {
                    optionButton[x,y] = new Button();
                    optionButton[x,y].SetBounds((160 * x) + 60, (30 * y) + 160 + 40, 100, 20);
                    optionButton[x,y].BackColor = Color.LightGray;
                    optionButton[x,y].FlatStyle = FlatStyle.Flat;
                    switch (y)
                    {
                        case 0:
                            optionButton[x, y].Text = "Withdraw";
                            optionButton[x, y].Click += new EventHandler(Withdraw_Click);
                            if (x == 1) { optionButton[x, y].Text = "Balance"; optionButton[x, y].Click += new EventHandler(Balance_Click); }
                            break;
                        case 1:
                            optionButton[x, y].Text = "Deposit";
                            optionButton[x, y].Click += new EventHandler(Deposit_Click);
                            if (x == 1) { optionButton[x, y].Text = "Change Pin"; optionButton[x, y].Click += new EventHandler(ChangePin_Click); }
                            break;
                        case 2:
                            optionButton[x, y].Text = "Cancel";
                            optionButton[x, y].Click += new EventHandler(Cancel_Click);
                            if (x == 1) { optionButton[x, y].Text = ""; }
                            break;
                    }
                    Controls.Add(optionButton[x,y]);
                }
            }
        }

        private void Withdraw_Click(object sender, EventArgs e)
        {
            //if (!showDataRace)
            //{
            //    sem.WaitOne();
            //}
            activeAccount.setBalance(CentralComp.getBalance(activeAccount.getAccountNum()));
            withdrawScreen();
            
        }

        private void Balance_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            keypad();
            activeAccount.setBalance(CentralComp.getBalance(activeAccount.getAccountNum()));

            Label display = new Label();
            display.Location = new Point(110, 100);
            display.Font = new Font("Ariel", 10);
            display.Text = "Balance: £" + (activeAccount.getBalance().ToString());
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

        private void Deposit_Click(object sender, EventArgs e)
        {
            depositClicked = true;
            otherClicked = false;
            changePinClicked = false;

            Controls.Clear();
            keypad();

            askUser.Text = "Enter Amount Depositing";
            askUser.Size = new Size(500, 300);
            askUser.Location = new Point(120, 170);
            Controls.Add(askUser);

            Box.Height = 200;
            Box.Width = 200;
            Box.Location = new Point(90, 190);
            Controls.Add(Box);
        }

        private void ChangePin_Click(object sender, EventArgs e)
        {
            depositClicked = false;
            otherClicked = false;
            changePinClicked = true;

            Controls.Clear();
            keypad();

            askUser.Text = "Enter new pin";
            askUser.Size = new Size(500, 300);
            askUser.Location = new Point(50, 100);
            Controls.Add(askUser);

            Box.Height = 200;
            Box.Width = 200;
            Box.Location = new Point(75, 190);
            Controls.Add(Box);
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
            CentralComp.updateAccount(activeAccount.getAccountNum(), activeAccount.getBalance());
            activeAccount = null;
            StartScreen();
        }

        //private int showBalance()
        //{
        //    return activeAccount.getBalance();
        //}

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
                    withdrawOptions[x, y].SetBounds((160 * x) + 60, (30 * y) + 160 + 40, 100, 20);
                    withdrawOptions[x, y].BackColor = Color.LightGray;
                    withdrawOptions[x, y].TabStop = false;
                    withdrawOptions[x, y].FlatStyle = FlatStyle.Flat;
                    withdrawOptions[x, y].FlatAppearance.BorderSize = 1;
                   switch (y)
                    {
                        case 0:
                            withdrawOptions[x, y].Text = "£10";
                            withdrawOptions[x, y].Name = "10";
                            if (x == 1) { withdrawOptions[x, y].Text = "£100"; withdrawOptions[x, y].Name = "100"; }
                            break;
                        case 1:
                            withdrawOptions[x, y].Text = "£20";
                            withdrawOptions[x, y].Name = "20";
                            if (x == 1) { withdrawOptions[x, y].Text = "£500"; withdrawOptions[x, y].Name = "500"; }
                            break;
                        case 2:
                            withdrawOptions[x, y].Text = "£40";
                            withdrawOptions[x, y].Name = "40";
                            if (x == 1) { withdrawOptions[x, y].Text = "Other"; withdrawOptions[x, y].Name = "Other"; }
                            break;
                    }
                    withdrawOptions[x, y].MouseDown += new MouseEventHandler(this.withdrawOptionsEvent_MouseDown);
                    Controls.Add(withdrawOptions[x, y]);
                }
            }
        }

        private void withdrawOptionsEvent_MouseDown(object sender, MouseEventArgs e)
        {
            string buttonName = ((Button)sender).Name;

            if (buttonName == "Other")
            {
                otherClicked = true;
                depositClicked = false;
                changePinClicked = false;
                Controls.Clear();
                keypad();

                askUser.Text = "Enter amount";
                askUser.Location = new Point(110, 100);
                Controls.Add(askUser);

                Box.Location = new Point(75, 190);
                Box.Height = 200;
                Box.Width = 200;
                Controls.Add(Box);
            }
            else
            {

                if (activeAccount.getBalance() > int.Parse(((Button)sender).Name))
                {
                    if (!showDataRace)
                    {
                        sem.WaitOne();
                        activeAccount.setBalance(CentralComp.getBalance(activeAccount.getAccountNum()));
                    }
                    activeAccount.decrementBalance(int.Parse(((Button)sender).Name));
                    CentralComp.updateAccount(activeAccount.getAccountNum(), activeAccount.getBalance());
                    if (!showDataRace)
                    {
                        sem.Release();
                    }

                    bill.SizeMode = PictureBoxSizeMode.StretchImage;
                    bill.Size = new Size(100, 200);
                    bill.Location = new Point(20, 420);
                    bill.Image = Image.FromFile("Bill.jpg");
                    bill.SendToBack();
                    Controls.Add(bill);

                    timer.Interval = 100;
                    timer.Tick += timer_Tick;
                    timer.Enabled = true;
                    timer.Start();
                    Thread.Sleep(3000);
                    //Task.Delay(5000).ContinueWith(t =>options());
                }    
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            MoveObject();
        }

        private void MoveObject()
        {
            int x = bill.Location.X;
            int y = bill.Location.Y;
            bill.Location = new Point(x, y+1);

            if (y == yfinal) { timer.Stop(); options(); }
        }

        private void ATM_Load(object sender, EventArgs e)
        {

        }

        static void Main()
        {
            CentralComp.setupCentralComp();
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

        public void incrementBalance(int amount)
        {
            this.balance += amount;
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

        internal int getPIN()
        {
            return pin;
        }

        public void setPin(int newPin)
        {
            pin = newPin;
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
                    return new Account(ac[i].getBalance(), ac[i].getPIN(), ac[i].getAccountNum());
                }
            }
            return null;
        }

        public static int getBalance(int acNum)
        {
            for (int i = 0; i < ac.Length; i++)
            {
                if (ac[i].getAccountNum() == acNum)
                {
                    return ac[i].getBalance();
                }
            }
            return 0;
        }

        public static void updateAccount(int accNum, int newBalance)
        {
            for (int i = 0; i < ac.Length; i++)
            {
                if (ac[i].getAccountNum() == accNum)
                {
                    ac[i].setBalance(newBalance);
                    return;
                }
            }
        }

    }

}