using Domain;

namespace ConsoleUI;

public static class Ui
{
    public static void DrawGameBoard(EGameTileState?[][] board, int selectedX, int selectedY)
    {
        var cols = board.GetLength(0);
        var rows = board[0].GetLength(0);

        var color = false;
        
        for (var i = 0; i < rows; i++)
        {
            color = !color;
            for (var j = 0; j < cols; j++)
            {
                Console.Write("+---");
            }
            Console.WriteLine("+");

            for (var j = 0; j < cols; j++)
            {
                Console.Write("|");
                if (!color)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    color = true;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    color = false;
                }

                Console.Write(" ");
                var pieceStr = " ";
                if (board[j][i] == EGameTileState.BlackStandard)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    pieceStr = "0";
                }
                else if (board[j][i] == EGameTileState.RedStandard)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    pieceStr = "O";
                }
                
                else if (board[j][i] == EGameTileState.PossibleMove)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                }
                else if (board[j][i] == EGameTileState.ForcedMove)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                }
                else if (board[j][i] == EGameTileState.SelectedBlackStandard)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    pieceStr = "O";
                }
                else if (board[j][i] == EGameTileState.SelectedRedStandard)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    pieceStr = "O";
                }
                else if (board[j][i] == EGameTileState.RedKing)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    pieceStr = "K";                }
                else if (board[j][i] == EGameTileState.BlackKing)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    pieceStr = "K";                  
                }
                else if (board[j][i] == EGameTileState.SelectedRedKing)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    pieceStr = "K";                  
                }
                else if (board[j][i] == EGameTileState.SelectedBlackKing)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    pieceStr = "K";                  
                }
                
                if (i == selectedY && j == selectedX)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                }

                Console.Write(pieceStr);
                Console.ResetColor();
                
                if (!color)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(" ");
                Console.BackgroundColor = ConsoleColor.Black;
                
            }
            Console.WriteLine("|");
            if (cols % 2 == 0) continue;
            color = !color;
        }

        for (int j = 0; j < cols; j++)
        {
            Console.Write("+---");
        }
        Console.WriteLine("+");
    }
}
