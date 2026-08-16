/* 
* NDS203 Assessment 3
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Windows_Forms_Chat
{
    public enum TileType
    {
        blank, cross, naught
    }
    public enum GameState
    {
        playing, draw, crossWins, naughtWins
    }

    public class TicTacToe
    {
        //TODO change myTurn to false and playerTileType to blank for defaults
        //they should be dictated by the server
        public bool myTurn = false;
        public TileType playerTileType = TileType.blank;
        public List<Button> buttons = new List<Button>();//assuming 9 in order
        public TileType[] grid = new TileType[9];

        public string GridToString()
        {
            string s = "";
            //TODO convert values on board to a string e.g "xox___x_o"
            for(int i=0; i < grid.Length; i++)
            {
                if(grid[i] == TileType.cross)
                {
                    s += "x";
                }
                else if(grid[i] == TileType.naught)
                {
                    s += "o";
                }
                else
                {
                    s += "_";
                }
            }//end for i

            return s;
        }//end GridToString

        public void StringToGrid(string s)
        {
            //TODO take string s e.g "xox___x_o" and use its values to update grid and the buttons
            if(s.Length != 9) return;

            for(int i=0; i < s.Length; i++)
            {
                if(s[i] == 'x')
                {
                    grid[i] = TileType.cross;
                }
                else if(s[i] == 'o')
                {
                    grid[i] = TileType.naught;
                }
                else
                {
                    grid[i] = TileType.blank;
                }
                if (buttons.Count >= 9) buttons[i].Text = TileTypeToString(grid[i]);
            }//end for
        }//end StringToGrid

        /// <summary>
        /// try and set the type
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tileType"></param>
        /// <returns>false if move was not valid</returns>
        public bool SetTile(int index, TileType tileType)
        {
            if (index < 0 || index >= grid.Length) return false;
            if (tileType == TileType.blank) return false;
            if (grid[index] == TileType.blank)
            {
                grid[index] = tileType;
                if (buttons.Count >= 9) buttons[index].Text = TileTypeToString(tileType);
                return true;
            }
            return false;//not valid
        }//end settile

        public GameState GetGameState()
        {
            GameState state = GameState.playing;
            if (CheckForWin(TileType.cross))
                state = GameState.crossWins;
            else if (CheckForWin(TileType.naught))
                state = GameState.naughtWins;
            else if (CheckForDraw())
                state = GameState.draw;


            return state;
        }
        public bool CheckForWin(TileType t)
        {
            //horizontals
            if (grid[0] == t && grid[1] == t && grid[2] == t)
                return true;
            if (grid[3] == t && grid[4] == t && grid[5] == t)
                return true;
            if (grid[6] == t && grid[7] == t && grid[8] == t)
                return true;

            //verticals
            if (grid[0] == t && grid[3] == t && grid[6] == t)
                return true;
            if (grid[1] == t && grid[4] == t && grid[7] == t)
                return true;
            if (grid[2] == t && grid[5] == t && grid[8] == t)
                return true;

            //diagonals
            if (grid[0] == t && grid[4] == t && grid[8] == t)
                return true;
            if (grid[2] == t && grid[4] == t && grid[6] == t)
                return true;


            //nothing
            return false;
        }

        public bool CheckForDraw()
        {
            for(int i = 0; i < 9; i++)
            {
                if (grid[i] == TileType.blank)
                    return false;
            }

            return true;
        }

        public void ResetBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                grid[i] = TileType.blank;
                if (buttons.Count >= 9)
                    buttons[i].Text = TileTypeToString(TileType.blank);
            }
        }

        public static string TileTypeToString(TileType t)
        {
            if (t == TileType.blank)
                return "";
            else if (t == TileType.cross)
                return "X";
            else
                return "O";
        }
    }
}
