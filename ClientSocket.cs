/* 
* NDS203 Assessment 2
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Windows_Forms_Chat
{
    public enum ClientState
    {
        Login,
        Chatting,
        Playing
    }//end enum

    /// <summary>
    /// Cli
    /// </summary>
    public class ClientSocket
    {
        //add other attributes to this, e.g username, what state the client is in etc
        public Socket socket;
        public const int BUFFER_SIZE = 2048;
        public byte[] buffer = new byte[BUFFER_SIZE];

        public string username = string.Empty;

        public bool bIsModerator = false;
        public ClientState state = ClientState.Login;
        public ClientSocket opponent = null;
        public TileType tileType = TileType.blank;
        public bool myTurn = false;
        public TicTacToe game = null;
    }//end class
}//end namespace
