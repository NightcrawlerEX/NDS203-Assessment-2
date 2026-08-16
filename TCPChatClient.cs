/* 
* NDS203 Assessment 3
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

//reference: https://github.com/AbleOpus/NetworkingSamples/blob/master/MultiClient/Program.cs
namespace Windows_Forms_Chat
{
    public class TCPChatClient : TCPChatBase
    {
        //public static TCPChatClient tcpChatClient;
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ClientSocket clientSocket = new ClientSocket();


        public int serverPort;
        public string serverIP;

        public string _username;
        public string _password;
        public bool _isRegistering;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="serverPort"></param>
        /// <param name="serverIP"></param>
        /// <param name="chatTextBox"></param>
        /// <param name="preferredUsername"></param>
        /// <returns></returns>
        public static TCPChatClient CreateInstance(int port, 
        int serverPort, string serverIP, TextBox chatTextBox, string username,
        string password, bool isRegistering)
        {
            TCPChatClient tcp = null;
            //if port values are valid and ip worth attempting to join
            if (port > 0 && port < 65535 && 
                serverPort > 0 && serverPort < 65535 && 
                serverIP.Length > 0 &&
                chatTextBox != null)
            {
                tcp = new TCPChatClient();
                tcp.port = port;
                tcp.serverPort = serverPort;
                tcp.serverIP = serverIP;
                tcp.chatTextBox = chatTextBox;
                tcp.clientSocket.socket = tcp.socket;
                tcp._username = username;
                tcp._password = password;
                tcp._isRegistering = isRegistering;
            }

            return tcp;
        }//end CreateInstance

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxAttempts"></param>
        public void ConnectToServer(int maxAttempts = 5)
        {
            int attempts = 0;

            while (!socket.Connected && attempts < maxAttempts)
            {
                try
                {
                    attempts++;
                    SetChat("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    socket.Connect(serverIP, serverPort);
                }
                catch (SocketException)
                {
                    chatTextBox.Text = "";
                }
            }
            if (!socket.Connected)
            {
                AddToChat("Failed to connect to server.");
                socket.Close();
                return;
            }
            //Console.Clear();
            AddToChat("Connected");
            //keep open thread for receiving data
            clientSocket.socket.BeginReceive(clientSocket.buffer,
             0, ClientSocket.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, 
             clientSocket);
            if (_isRegistering)
            {
                SendString("!register " + _username + " " + _password);
            }
            else
            {
                SendString("!login " + _username + " " + _password);
            }
        }//end ConnectToServer


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SendString(string text)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(text);
                socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch
            {
                AddToChat("SERVER: You are not connected.");
                socket.Close();
            }
            
        }//end SendString

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AR"></param>
        public void ReceiveCallback(IAsyncResult AR)
        {
            ClientSocket currentClientSocket = (ClientSocket)AR.AsyncState;

            int received;

            try
            {
                received = currentClientSocket.socket.EndReceive(AR);
                if (received == 0)
                {
                    AddToChat("SERVER: Disconnected from server.");
                    currentClientSocket.socket.Close();
                    return;
                }
            }
            catch (SocketException)
            {
                AddToChat("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                currentClientSocket.socket.Close();
                return;
            }
            //read bytes from packet
            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received);
            //convert to string so we can work with it
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);

            if (text == "!register_success")
            {
                AddToChat("SERVER: Registration successful.");
                // Registration succeeded, so now log in.
                _isRegistering = false;
                SendString("!login " + _username + " " + _password);
            }
            else if (text.StartsWith("!register_failed"))
            {
                string reason = text.Substring("!register_failed".Length).Trim();
                if (string.IsNullOrWhiteSpace(reason)) reason = "Registration failed.";
                AddToChat("SERVER: " + reason);
            }
            else if (text == "!login_success")
            {
                clientSocket.state = ClientState.Chatting;
                AddToChat("SERVER: Login successful.");
            }
            else if (text.StartsWith("!login_failed"))
            {
                string reason = text.Substring("!login_failed".Length).Trim();
                if (string.IsNullOrWhiteSpace(reason)) reason = "Login failed.";
                AddToChat("SERVER: " + reason);
            }
            else
            {
                // Normal chat message from the server.
                AddToChat(text);
            }
            
            try {
            //we just received a message from this socket, better keep an ear out with another thread for the next one
            currentClientSocket.socket.BeginReceive(currentClientSocket.buffer, 0, ClientSocket.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, currentClientSocket);
            }
            catch
            {
                AddToChat("SERVER: Disconnected from server.");
                currentClientSocket.socket.Close();
            }
        }//end ReceiveCallBack

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            socket.Close();
        }//end close
    }//end class

}//end namespace
