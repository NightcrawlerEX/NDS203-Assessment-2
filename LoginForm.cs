/* 
* NDS203 Assessment 2
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
        Button _okButton;

        public LoginForm()
        {
            this.Width = 300;
            this.Height = 200;
            this.Text = "Login";
            Label signinLabel = new Label();
            signinLabel.Location = new Point(25,25);
            signinLabel.Text = "Enter Username";
            this.Controls.Add(signinLabel);

            _usernameTextBox = new TextBox();
            _usernameTextBox.Width = 250;
            _usernameTextBox.Location = new Point(25,50);
            this.Controls.Add(_usernameTextBox);

            _okButton = new Button();
            _okButton.Text = "Login";
            _okButton.Location = new Point(50,100);
            _okButton.Click += okButton_Click;
            this.Controls.Add(_okButton);

        }//end constructor

        public string Value
        {
            get { return _usernameTextBox.Text;}
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

    }//end class
}//end namespace