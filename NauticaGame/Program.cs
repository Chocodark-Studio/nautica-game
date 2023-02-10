using System;

namespace NauticaGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int boardSize = 0;

            // input until the user types a valid size
            while (boardSize <= 1)
            {
                Console.Write("\nEnter board size: (3 is the best)\n_ ");
                string inputSize = Console.ReadLine();

                int.TryParse(inputSize, out boardSize);
            }


            // starts new nautica game
            NauticaGameBoard nauticaGame = new NauticaGameBoard(boardSize);
            nauticaGame.StartNewGame();

            // string for read the console 
            string xIn, yIn;

            nauticaGame.PrintBoard(clearConsole: true);
            while (nauticaGame.State == GameState.PLAYING)
            {
                Console.Write("\n\nEnter the Horizontal coordinate:\n_ ");
                xIn = Console.In.ReadLine();
                Console.Write("\nEnter the Vertical coordinate:\n_ ");
                yIn = Console.In.ReadLine();

                // if converting inputs to int fails, ask again
                if (!int.TryParse(xIn, out int x))
                    continue;
                if (!int.TryParse(yIn, out int y))
                    continue;


                bool dropped = nauticaGame.DropABomb(x, y);

                // failed to drop a bomb, try again
                if (!dropped)
                    continue;

                // if still playing (dosnt win or lose)
                if (nauticaGame.State == GameState.PLAYING)
                {
                    nauticaGame.PrintBoard(clearConsole: true);
                    Console.WriteLine("\nAttempts remaining: " + nauticaGame.attempts);
                }
            }

            nauticaGame.ClearConsole();
            nauticaGame.board.PrintReferences();
            nauticaGame.PrintBoard(clearConsole: false);

            // final message
            Console.Write("\n");
            if (nauticaGame.State == GameState.WIN)
                Console.WriteLine("You have win! :)");
            else
                Console.WriteLine("You have lose :(");

            // prevent auto-close console
            Console.ReadLine();
        }
    }
}