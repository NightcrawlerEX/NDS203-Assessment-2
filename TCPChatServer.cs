/* 
* NDS203 Assessment 2
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

//https://github.com/AbleOpus/NetworkingSamples/blob/master/MultiServer/Program.cs
namespace Windows_Forms_Chat
{
    public class TCPChatServer : TCPChatBase
    {
        
        public Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //connected clients
        public List<ClientSocket> clientSockets = new List<ClientSocket>();

        public static TCPChatServer createInstance(int port, TextBox chatTextBox)
        {
            TCPChatServer tcp = null;
            //setup if port within range and valid chat box given
            if (port > 0 && port < 65535 && chatTextBox != null)
            {
                tcp = new TCPChatServer();
                tcp.port = port;
                tcp.chatTextBox = chatTextBox;

            }

            //return empty if user not enter useful details
            return tcp;
        }

        public void SetupServer()
        {
            chatTextBox.Text += "Setting up server...\n";
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(0);
            //kick off thread to read connecting clients, when one connects, it'll call out AcceptCallback function
            serverSocket.BeginAccept(AcceptCallback, this);
            chatTextBox.Text += "Server setup complete\n";
        }



        public void CloseAllSockets()
        {
            foreach (ClientSocket clientSocket in clientSockets)
            {
                clientSocket.socket.Shutdown(SocketShutdown.Both);
                clientSocket.socket.Close();
            }
            clientSockets.Clear();
            serverSocket.Close();
        }

        public void AcceptCallback(IAsyncResult AR)
        {
            Socket joiningSocket;

            try
            {
                joiningSocket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            ClientSocket newClientSocket = new ClientSocket();
            newClientSocket.socket = joiningSocket;

            clientSockets.Add(newClientSocket);
            //start a thread to listen out for this new joining socket. Therefore there is a thread open for each client
            joiningSocket.BeginReceive(newClientSocket.buffer, 0, ClientSocket.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, newClientSocket);
            AddToChat("Client connected, waiting for request...");

            //we finished this accept thread, better kick off another so more people can join
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        public void ReceiveCallback(IAsyncResult AR)
        {
            ClientSocket currentClientSocket = (ClientSocket)AR.AsyncState;
            
            int received;

            try
            {
                received = currentClientSocket.socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                AddToChat("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                currentClientSocket.socket.Close();
                clientSockets.Remove(currentClientSocket);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            AddToChat( text );

            if(text.ToLower().StartsWith("!username"))
            {
                string proposedUsername = text.Substring(10).Trim();
                //first check if the username is null
                if (string.IsNullOrWhiteSpace(proposedUsername))
                {
                    SendToClient(currentClientSocket, "!username_failed Invalid username");
                    DisconnectClient(currentClientSocket);
                return;
                }//endif
                //if we get here username is not null
                //check for existing username
                bool bIsUsernameInUse = false;
                foreach(var client in clientSockets)
                {
                    if(client == currentClientSocket) continue;
                    if(client.username == proposedUsername)
                    {
                        bIsUsernameInUse = true;
                        break;
                    }
                }//end foreach
                if (bIsUsernameInUse)
                {
                    SendToClient(currentClientSocket, "!username_failed Username already in use");
                    DisconnectClient(currentClientSocket);
                    return;
                }
                currentClientSocket.username = proposedUsername;
                SendToClient(currentClientSocket, "!username_success");
                AddToChat(proposedUsername + " connected");
            }
            else if (text.ToLower() == "!commands") // Client requested time
            {
                byte[] data = Encoding.ASCII.GetBytes("Commands are !commands !about !who !whisper !exit");
                currentClientSocket.socket.Send(data);
                AddToChat("Commands sent to client");
            }
            else if (text.ToLower() == "!exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                currentClientSocket.socket.Shutdown(SocketShutdown.Both);
                currentClientSocket.socket.Close();
                clientSockets.Remove(currentClientSocket);
                AddToChat("Client disconnected");
                return;
            }
            else if (text.ToLower() == "!who")
            {
                string outputString = "Connected Users: \n";
                foreach(ClientSocket clientSocket in clientSockets)
                {
                    if(!string.IsNullOrWhiteSpace(clientSocket.username)) outputString += clientSocket.username;
                }
                SendToClient(currentClientSocket, outputString);
            }
            else if (text.ToLower() == "!about")
            {
                string outputString =
                    "Student ID: A00125081\n" +
                    "Student Name: James Simpson\n" +
                    "Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2";
                SendToClient(currentClientSocket, outputString);
            }
            else if(text.ToLower().StartsWith("!whisper"))
            {
                //reference https://www.programiz.com/csharp-programming/library/string/indexof
                string restOfCommand = text.Substring(9).Trim();//get the right hand side
                int spacePosition = restOfCommand.IndexOf(' ');//find the space
                //send them a message if they get the command wrong
                if (spacePosition == -1) { SendToClient(currentClientSocket, "Usage: !whisper username message"); return;}
                string username = restOfCommand.Substring(0, spacePosition);
                string message = restOfCommand.Substring(spacePosition + 1).Trim();
                //now find the target client
                ClientSocket target = null;
                foreach (ClientSocket client in clientSockets)
                {
                    if(client.username == username)
                    {
                        target = client;
                        break;
                    }
                }//end foreach
                //if could not find client send error
                if (target == null)
                {
                    SendToClient(currentClientSocket,"Could not find user: " + username);
                    return;
                }
                //if no message
                if (string.IsNullOrWhiteSpace(message))
                {
                    SendToClient(currentClientSocket,"You must include a message.");
                }
                //if we get here everything is good
                SendToClient(target, "[Whisper from " + currentClientSocket.username + "] " + message);
                SendToClient(currentClientSocket, "[Whisper to " + target.username + "] " + message);
                AddToChat(currentClientSocket.username + " whispered to " + target.username);
            }
            else if (text.ToLower() == "!time")
            {
                //this is the custom one
                string timeMessage = "Server time: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                SendToClient(currentClientSocket, timeMessage);
                AddToChat("Server time sent to " + currentClientSocket.username);
            }
            else if(text.ToLower() == "!mods")
            {
                
            }
            else if(text.ToLower().StartsWith("!mod"))
            {
                
            }
            else if (string.IsNullOrWhiteSpace(currentClientSocket.username))
            {
                SendToClient(
                currentClientSocket,
                "You must register a username before sending messages."
                );
            }
            else
            {
                string message = currentClientSocket.username + ": " + text;

                AddToChat(message);
                SendToAll(message, currentClientSocket);
            }
            //we just received a message from this socket, better keep an ear out with another thread for the next one
            currentClientSocket.socket.BeginReceive(currentClientSocket.buffer, 0, ClientSocket.BUFFER_SIZE, SocketFlags.None, ReceiveCallback, currentClientSocket);
        }

        /// <summary>
        /// Used to process a server command
        /// </summary>
        public void ProcessCommand(string command)
        {
            if(command.ToLower().StartsWith("!mod"))
            {
                //rip out the username
                string username = command.Substring(5).Trim();
                //find target
                ClientSocket target = null;
                foreach (ClientSocket client in clientSockets)
                {
                    if(client.username == username) {target = client; break;}
                }//end foreach
                //if could not be found then send error
                if (target == null)
                {
                    AddToChat("Could not find user: " + username);
                    return;
                }
                //flip the boolean
                target.bIsModerator = !target.bIsModerator;
                if (target.bIsModerator)
                {
                    AddToChat(target.username + " is now a moderator.");
                    SendToClient(target, "The server made you a moderator.");
                }
                else
                {
                    AddToChat(target.username + " is no longer a moderator.");
                    SendToClient(target, "The server removed your moderator status.");
                }
            }//endif
        }//end ProcessCommand

        public void SendToAll(string str, ClientSocket from)
        {
            foreach(ClientSocket c in clientSockets)
            {
                if(from == null || !from.socket.Equals(c))
                {
                    byte[] data = Encoding.ASCII.GetBytes(str);
                    c.socket.Send(data);
                }
            }
        }

        private void SendToClient(ClientSocket client, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            client.socket.Send(data);
        }//end SendToClient

        private void DisconnectClient(ClientSocket client)
        {
            clientSockets.Remove(client);

            try
            {
                client.socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
            }

            client.socket.Close();
        }//end DisconnectClient

        
    }//end class
}//end namespace
