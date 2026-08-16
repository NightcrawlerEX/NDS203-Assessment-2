/* 
* NDS203 Assessment 3
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
        private Ass3.Database _database = new Ass3.Database();
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

        /// <summary>
        /// Setup the server and create the database
        /// </summary>
        public void SetupServer()
        {
            chatTextBox.Text += "Create the database...\n";
            try {
            _database.CreateTable();
            }
            catch
            {
                chatTextBox.Text += "[Error] Failed to create Database\n";
                throw new Exception("Failed to create server");
            }
            chatTextBox.Text += "Setting up server...\n";
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(0);
            //kick off thread to read connecting clients, when one connects, it'll call out AcceptCallback function
            serverSocket.BeginAccept(AcceptCallback, this);
            chatTextBox.Text += "Server setup complete\n";
        }//end SetupServer



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
                if (received == 0)
                {
                    AddToChat("Client disconnected");
                    currentClientSocket.socket.Close();
                    clientSockets.Remove(currentClientSocket);
                    return;
                }
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

            /*if(text.ToLower().StartsWith("!username"))
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
            }*/
            //new register command
            if (currentClientSocket.state == ClientState.Login && text.ToLower().StartsWith("!register"))
            {
                //https://learn.microsoft.com/en-us/dotnet/api/system.string.split?view=net-10.0#system-string-split(system-char()-system-int32-system-stringsplitoptions)
                string[] registerDetails = text.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                if (registerDetails.Length != 3)
                {
                    SendToClient(currentClientSocket, "!register_failed Usage: !register username password");
                }
                else
                {
                    string username = registerDetails[1];
                    string password = registerDetails[2];
                    //try and create user
                    if(_database.CreateUser(username, password))
                    {
                        //if successful
                        SendToClient(currentClientSocket, "!register_success");
                        AddToChat("New user registered: " + username);
                    }
                    else//failed to create user
                    {
                        SendToClient(currentClientSocket, "!register_failed");
                    }//endif
                }
            }
            else if (currentClientSocket.state == ClientState.Login && text.ToLower().StartsWith("!login"))
            {
                //https://learn.microsoft.com/en-us/dotnet/api/system.string.split?view=net-10.0#system-string-split(system-char()-system-int32-system-stringsplitoptions)
                string[] loginDetails = text.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                if (loginDetails.Length != 3)
                {
                    SendToClient(currentClientSocket, "!login_failed Usage: !login username password");
                }
                else
                {
                    string username = loginDetails[1];
                    string password = loginDetails[2];
                    bool usernameInUse = false;
                    foreach(ClientSocket client in clientSockets)
                    {
                        if(client == currentClientSocket) continue;
                        if (client.state != ClientState.Login && client.username == username)
                        {
                            usernameInUse = true;
                            break;
                        }
                    }//end foreach
                    if(usernameInUse)
                    {
                        SendToClient(currentClientSocket, "!login_failed User is already logged in");
                    }
                    else//username not in use
                    {
                        if(_database.TryLogin(username, password))
                        {
                            currentClientSocket.username = username;
                            currentClientSocket.state = ClientState.Chatting;
                            SendToClient(currentClientSocket,"!login_success");
                            AddToChat(username + " logged in");
                        }
                        else //login not successful
                        {
                            SendToClient(currentClientSocket, "!login_failed Incorrect username or password");
                        }
                    }//endif
                }
            }
            else if (currentClientSocket.state == ClientState.Login) //login check
            {
                SendToClient(currentClientSocket, "SERVER: You must register or log in first.");
            }
            else if (text.ToLower() == "!commands") // Client requested time
            {
                byte[] data = Encoding.ASCII.GetBytes("Commands are !commands !about !who !whisper !time !exit !scores");
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
            else if(text.ToLower().StartsWith("!play"))
            {
                if (currentClientSocket.state != ClientState.Chatting)
                {
                    SendToClient(currentClientSocket, "SERVER: You cannot start a game now.");
                }
                else
                {
                    string username = text.Substring(5).Trim();
                    ClientSocket opponent = null;
                    foreach (ClientSocket client in clientSockets)
                    {
                        if (client.username == username)
                        {
                            opponent = client;
                            break;
                        }
                    }//end foreach
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        SendToClient(currentClientSocket, "Usage: !play username");
                    }
                    else if (opponent == null)
                    {
                        SendToClient(currentClientSocket, "SERVER: Couldn't find user: " + username);
                    }
                    else if (opponent == currentClientSocket)
                    {
                        SendToClient(currentClientSocket,"SERVER: Inalvid");
                    }
                    else if (opponent.state != ClientState.Chatting)
                    {
                        SendToClient(currentClientSocket, "SERVER: not available");
                    }
                    else//start the game
                    {
                        TicTacToe newGame = new TicTacToe();

                        currentClientSocket.opponent = opponent;
                        opponent.opponent = currentClientSocket;

                        currentClientSocket.game = newGame;
                        opponent.game = newGame;

                        currentClientSocket.tileType = TileType.cross;
                        opponent.tileType = TileType.naught;

                        currentClientSocket.myTurn = true;
                        opponent.myTurn = false;

                        currentClientSocket.state = ClientState.Playing;
                        opponent.state = ClientState.Playing;

                        SendToClient(currentClientSocket, "!game_start cross " + opponent.username);

                        SendToClient(opponent, "!game_start naught " + currentClientSocket.username);

                        SendToClient(currentClientSocket, "!turn");

                        SendToClient(opponent, "!wait");

                        AddToChat(currentClientSocket.username + " started a game with " + opponent.username);
                    }
                }//endif
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
            else if(text.ToLower().StartsWith("!scores"))
            {
                try
                {
                    string scores = _database.GetScores();
                    SendToClient(currentClientSocket, scores);
                    AddToChat("Scores sent to: " + currentClientSocket.username);
                }
                catch
                {
                    SendToClient(currentClientSocket, "Exception: failed to get scores");
                    AddToChat("Exception: failed to get scores");
                }
            }
            else if (string.IsNullOrWhiteSpace(currentClientSocket.username))
            {
                SendToClient(
                currentClientSocket,
                "You must register a username before sending messages."
                );
            }
            else if(text.ToLower().StartsWith("!kick"))
            {
                if (!currentClientSocket.bIsModerator)
                {
                    SendToClient(currentClientSocket, "Only moderators can kick users.");
                    return;
                }
                //get the username
                string username = text.Substring(6).Trim();
                ClientSocket target = null;
                foreach (ClientSocket client in clientSockets)
                {
                    if (client.username == username){target = client; break; }
                }//end foreach
                if (target == null)//if could not find
                {
                    SendToClient(currentClientSocket,"Could not find user: " + target);
                    return;
                }//endif
                //if we get here kick the target
                SendToClient(target, "You were kicked by moderator " + currentClientSocket.username);
                DisconnectClient(target);
                SendToClient(currentClientSocket, target + " was kicked.");
                AddToChat(target + " was kicked by " + currentClientSocket.username);
            }
            else if (text.ToLower().StartsWith("!move"))
            {
                if (currentClientSocket.state != ClientState.Playing ||
                    currentClientSocket.game == null ||
                    currentClientSocket.opponent == null)
                {
                    SendToClient(currentClientSocket, "SERVER: You are not currently playing.");
                }
                else if (!currentClientSocket.myTurn)
                {
                    SendToClient(currentClientSocket, "SERVER: It is not your turn.");
                }
                else
                {
                    string moveText = text.Substring(5).Trim();
                    int index;

                    if (!int.TryParse(moveText, out index))
                    {
                        SendToClient(currentClientSocket, "Usage: !move index");
                    }
                    else
                    {
                        bool validMove = currentClientSocket.game.SetTile(index, currentClientSocket.tileType);

                        if (!validMove)
                        {
                            SendToClient(currentClientSocket, "SERVER: That move is not valid." );
                        }
                        else
                        {
                            ClientSocket opponent = currentClientSocket.opponent;

                            string board = currentClientSocket.game.GridToString();
                            SendToClient( currentClientSocket, "!board " + board );
                            SendToClient(opponent, "!board " + board);

                            GameState gameState = currentClientSocket.game.GetGameState();

                            if (gameState == GameState.playing)
                            {
                                currentClientSocket.myTurn = false;
                                opponent.myTurn = true;
                                SendToClient( currentClientSocket,"!wait");
                                SendToClient(opponent,"!turn" );
                            }
                            else if (gameState == GameState.draw)
                            {
                                try
                                {
                                    _database.RecordDraw(currentClientSocket.username);
                                    _database.RecordDraw(opponent.username);
                                }
                                catch
                                {
                                    AddToChat("Exception. Failed to add draw");
                                }
                                SendToClient(currentClientSocket,"!game_end draw");
                                SendToClient(opponent, "!game_end draw");
                                EndGame(currentClientSocket,opponent );
                            }
                            else
                            {
                                try
                                {
                                    _database.RecordWin(currentClientSocket.username);
                                    _database.RecordLoss(opponent.username);
                                }
                                catch
                                {
                                    AddToChat("Exception. Failed to save result");
                                }
                                SendToClient(currentClientSocket,"!game_end win" );
                                SendToClient(opponent,"!game_end lose");
                                EndGame(currentClientSocket,opponent);
                            }
                        }
                    }
                }
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
            //MessageBox.Show("ProcessCommand received: " + command);
            if(command.ToLower() == "!mods")
            {
                string outputString = "Current moderators:";
                bool bDoModeratorsExist = false;
                foreach (ClientSocket client in clientSockets)
                {
                    if (client.bIsModerator)
                    {
                        outputString += "\n" + client.username;
                        bDoModeratorsExist = true;
                    }
                }//end foreac
                if (!bDoModeratorsExist)//if no moderators exist
                {
                    outputString += "\nNo moderators connected.";
                }
                AddToChat(outputString);
            }
            else if(command.ToLower().StartsWith("!mod"))
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
            }
            else //bad command
            {
                AddToChat("Unknown server command.");
            }
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

        private void EndGame(ClientSocket playerOne, ClientSocket playerTwo)
        {
            playerOne.state = ClientState.Chatting;
            playerTwo.state = ClientState.Chatting;

            playerOne.myTurn = false;
            playerTwo.myTurn = false;

            playerOne.tileType = TileType.blank;
            playerTwo.tileType = TileType.blank;

            playerOne.game = null;
            playerTwo.game = null;

            playerOne.opponent = null;
            playerTwo.opponent = null;
        }//end EndGame
        
    }//end class
}//end namespace
