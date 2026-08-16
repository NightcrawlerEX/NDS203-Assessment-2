/* 
* NDS203 Assessment 3
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Forms_Chat
{
    public partial class LoginForm : Form
    {
        TextBox _usernameTextBox;
        TextBox _passwordTextBox;
        Button _loginButton;
        Button _registerButton;

        public LoginForm()
        {
            this.Width = 320;
            this.Height = 250;
            this.Text = "Login or Register";

            Label usernameLabel = new Label();
            usernameLabel.Location = new Point(25, 20);
            usernameLabel.Text = "Username";
            this.Controls.Add(usernameLabel);

            _usernameTextBox = new TextBox();
            _usernameTextBox.Width = 250;
            _usernameTextBox.Location = new Point(25, 45);
            this.Controls.Add(_usernameTextBox);

            Label passwordLabel = new Label();
            passwordLabel.Location = new Point(25, 80);
            passwordLabel.Text = "Password";
            this.Controls.Add(passwordLabel);

            _passwordTextBox = new TextBox();
            _passwordTextBox.Width = 250;
            _passwordTextBox.Location = new Point(25, 105);
            _passwordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(_passwordTextBox);

            _loginButton = new Button();
            _loginButton.Text = "Login";
            _loginButton.Location = new Point(25, 150);
            _loginButton.Click += loginButton_Click;
            this.Controls.Add(_loginButton);

            _registerButton = new Button();
            _registerButton.Text = "Register";
            _registerButton.Location = new Point(125, 150);
            _registerButton.Click += registerButton_Click;
            this.Controls.Add(_registerButton);
        }//end constructor

        public string Username
        {
            get { return _usernameTextBox.Text;}
        }
        public string Password
        {
            get { return _passwordTextBox.Text; }
        }
        public bool IsRegistering { get; private set; }

        private void loginButton_Click(object sender, EventArgs e)
        {
            IsRegistering = false;
            DialogResult = DialogResult.OK;
            Close();
        }//end loginButton_Click

        private void registerButton_Click(object sender, EventArgs e)
        {
            IsRegistering = true;
            DialogResult = DialogResult.OK;
            Close();
        }//end registerButton_Click

    }//end class
}//end namespace