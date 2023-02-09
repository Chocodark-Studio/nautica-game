using System;
using System.Collections.Generic;

namespace NauticaGame
{

    public enum GameState { NOT_STARTED, PLAYING, WIN, LOSE };

    /// <summary>
    /// Manage the flow and state of the game </summary>
    public class NauticaGameBoard
    {
        public readonly NauticaBoard board;

        // attempts count
        public int attempts;
        public int maxAttempts;

        GameState gameState = GameState.NOT_STARTED;

        // getter
        public GameState State => gameState;

        public enum Difficulty { EASY, NORMAL, HARD };
        public Difficulty difficulty;


        public NauticaGameBoard(int boardSize, Difficulty difficulty = Difficulty.NORMAL)
        {
            board = new NauticaBoard(boardSize);
            this.difficulty = difficulty;
        }


        public void StartNewGame(int maxAttempts = -1)
        {
            // default parameter, set given maxAttempts to diagnoal board size
            if (maxAttempts <= 0)
                maxAttempts = board.boardSize;

            this.maxAttempts = maxAttempts;

            // reload attempts
            attempts = this.maxAttempts;

            gameState = GameState.PLAYING;

            // set probability by difficulty
            float shipProbability;
            switch (difficulty)
            {
                default:
                case Difficulty.EASY:
                    shipProbability = 0.8f;
                    break;
                case Difficulty.NORMAL:
                    shipProbability = 0.5f;
                    break;
                case Difficulty.HARD:
                    shipProbability = 0.2f;
                    break;
            }

            board.Randomize(shipProbability);
        }


        /// <summary>
        /// Tries to drop a bomb on the board, returns true if able </summary>
        public bool DropABomb(int x, int y)
        {
            // isnt playing
            if (gameState != GameState.PLAYING)
                return false;


            // if no more ships to destroy, end the game
            if (!board.AreShipsToDestroy())
            {
                // win the game
                gameState = GameState.WIN;
                // cancel attempt
                return false;
            }
            // not enough attemps
            else if (attempts <= 0)
            {
                // lose the game
                gameState = GameState.LOSE;
                // cancel attempt
                return false;
            }

            // try to drop
            bool dropped = board.TryDropABomb(x, y);

            // only substact attempts if its a failed
            if (dropped && board.Compare(x, y, 2))
                attempts--;

            // if no more ships to destroy, end the game
            if (!board.AreShipsToDestroy())
                // win the game
                gameState = GameState.WIN;

            else if (attempts <= 0)
                // lose the game
                gameState = GameState.LOSE;

            return dropped;
        }


        public void PrintBoard(bool clearConsole = false)
        {
            if (clearConsole)
                ClearConsole();

            // print board, in secret if playing
            board.Print(gameState == GameState.PLAYING);
        }


        /// <summary>
        /// Write a series of new lines in the console to scroll-out the previous board </summary>
        public void ClearConsole()
        {
            // number of new lines depends on the height of the console, but 80 is OK
            for (int i = 0; i < 80; i++)
                Console.Write("\n");
        }
    }


    /// <summary>
    /// Manages board matrix and its operations </summary>
    public class NauticaBoard
    {
        /// <summary>
        /// diagonal size of the board </summary>
        public readonly int boardSize;

        readonly int[,] board;

        // counters
        public int shipsCount = 0;
        public int droppedBombsCount = 0;
        public int successfullyBombsCount = 0;


        /// <summary>
        /// String representation of an board code </summary>
        readonly Dictionary<int, string> boardCodes = new Dictionary<int, string>() {
            {0, " "}, // water
            {1, "="}, // ship
            {2, "."}, // failed attempt
            {3, "X"}, // successful attempt
            {4, "?"}  // incognito
        };


        /// <summary>
        /// Color representation of an board code </summary>
        readonly Dictionary<int, ConsoleColor> boardColorCodes = new Dictionary<int, ConsoleColor>() {
            {0, ConsoleColor.Black},        // water
            {1, ConsoleColor.DarkGray},     // ship
            {2, ConsoleColor.DarkRed},      // failed attempt
            {3, ConsoleColor.DarkGreen},    // successful attempt
            {4, ConsoleColor.Black}         // incognito
        };


        public NauticaBoard(int size = 2)
        {
            boardSize = size;

            // create board matrix
            board = new int[boardSize, boardSize];
        }


        /// <summary>
        /// Clears the board and randomize it with ships </summary>
        public void Randomize(float shipProbability = 0.2f)
        {
            Random rand = new Random();

            // ships added to the board
            shipsCount = 0;

            // randomize it, until generated ships are more than a percent of board
            while (shipsCount < board.Length * shipProbability)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        // by default put water
                        board[y, x] = 0;

                        // if probability, put a ship
                        if (rand.NextDouble() <= shipProbability)
                        {
                            board[y, x] = 1;
                            shipsCount++;
                        }
                    }
                }
            }

            // reset succesfully attempts count
            successfullyBombsCount = 0;
        }


        /// <summary>
        /// Convert a number into a string with specified digits </summary>
        string Digit(int num, int digits = 2)
        {
            string result = "";
            int missingDigits = digits - num.ToString().Length;

            // adds '0' character in front of num, the times the digits are missing
            for (int i = 0; i < missingDigits; i++)
                result += "0";

            // add the number at final position and return it
            return result + num;
        }


        /// <summary>
        /// Print the board in the console </summary>
        /// 
        /// <param name="hide">
        /// hide water and ships with incognito code </param>
        public void Print(bool hide = true)
        {
            string separator = "|";
            string border = "-";
            string incognito = boardCodes[4];


            // print spaces to correct horizontal alignment
            Console.Write("    ");
            for (int i = 0; i < boardSize.ToString().Length - 1; i++)
            {
                Console.Write(" ");
            }

            // horizontal index sequence
            for (int i = 0; i < boardSize; i++)
            {
                Console.Write((i) + " ");
            }
            Console.Write("\n");


            // print spaces to correct horizontal alignment
            Console.Write("   ");
            for (int i = 0; i < boardSize.ToString().Length - 1; i++)
            {
                Console.Write(" ");
            }

            // top borders
            for (int i = 0; i < boardSize * 2 + 1; i++)
            {
                Console.Write(border);
            }
            Console.Write("\n");


            for (int y = 0; y < boardSize; y++)
            {
                // vertical index sequence
                Console.Write(" " + Digit(y, boardSize.ToString().Length) + " " + separator);

                for (int x = 0; x < boardSize; x++)
                {
                    // if hide is true and there is water or ship below
                    if (hide
                        && (board[y, x] == 0 || board[y, x] == 1))
                    {
                        // replace it with incognito code
                        ColoredConsole.Print(incognito, boardColorCodes[4]);
                    }
                    else
                    {
                        ColoredConsole.Print(boardCodes[board[y, x]], boardColorCodes[board[y, x]]);
                    }

                    Console.Write(separator);
                }
                Console.Write("\n");
            }


            // print spaces to correct horizontal alignment
            for (int i = 0; i < boardSize.ToString().Length - 1; i++)
            {
                Console.Write(" ");
            }

            // bottom borders
            Console.Write("   ");
            for (int i = 0; i < boardSize * 2 + 1; i++)
            {
                Console.Write(border);
            }
            Console.Write("\n");
        }


        /// <summary>
        /// Sets a value in the board only if its a water or ship below </summary>
        /// 
        /// <returns>
        /// Returns true if the value could be set </returns>
        public bool TryDropABomb(int x, int y)
        {
            if (!IsValidCoor(x, y))
                return false;

            // by default set a failed attempt code
            int code = 2;

            // if its a ship below, code is successful
            if (Compare(x, y, 1))
            {
                code = 3;
                // count a successful attempt
                successfullyBombsCount++;
            }
            // if isnt a ship and isnt water, cannot drop a bomb
            else if (!Compare(x, y, 0))
                return false;

            Set(x, y, code);

            // increase the 'bombs dropped' count
            droppedBombsCount++;
            return true;
        }

        public void Set(int x, int y, int code)
        {
            if (!IsValidCoor(x, y))
                return;

            board[y, x] = code;
        }


        /// <summary>
        /// Checks if the given coordinates dont go outside the limits of the board </summary>
        public bool IsValidCoor(int x, int y)
        {
            return (boardSize - 1) - x >= 0
                && (boardSize - 1) - y >= 0;
        }


        /// <param name="code">Board code that will be compared</param>
        public bool Compare(int x, int y, int code)
        {
            if (!IsValidCoor(x, y))
                return false;

            return board[y, x] == code;
        }


        /// <summary>
        /// return true if there is a ship that hasnt been destroyed. </summary>
        public bool AreShipsToDestroy()
        {
            if (shipsCount <= 0)
                return false;

            return successfullyBombsCount < shipsCount;
        }


        /// <summary>
        /// Print board codes with their color and meaning </summary>
        public void PrintReferences()
        {
            Console.WriteLine("References:");

            ColoredConsole.Print(boardCodes[0], boardColorCodes[0]);
            Console.WriteLine(" -> Water");

            ColoredConsole.Print(boardCodes[1], boardColorCodes[1]);
            Console.WriteLine(" -> Ship");

            ColoredConsole.Print(boardCodes[2], boardColorCodes[2]);
            Console.WriteLine(" -> Failed attempt");

            ColoredConsole.Print(boardCodes[2], boardColorCodes[3]);
            Console.WriteLine(" -> Successful attempt\n\n");
        }
    }
}

/// <summary>
/// Extra class: Print a colored messages in the console </summary>
public class ColoredConsole
{
    /// <summary>
    /// Print a colored message in the console </summary>
    public static void Print(object obj, ConsoleColor color = ConsoleColor.Black)
    {
        // get previous console background color
        ConsoleColor previusColor = Console.BackgroundColor;

        // default color is the previous one
        if (color == ConsoleColor.Black)
            color = previusColor;

        // set the console with given background color
        Console.BackgroundColor = color;

        // print the msg
        Console.Write(obj);

        // reset console background color
        Console.BackgroundColor = previusColor;
    }

    /// <summary>
    /// Print a colored message in the console with newLine </summary>
    public static void PrintLine(object obj, ConsoleColor color = ConsoleColor.Black)
    {
        Print(obj, color);
        Console.Write("\n");
    }
}
