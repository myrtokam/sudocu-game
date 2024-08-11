namespace SudokuGame;

public class Sudoku
{
    public int[,] Board { get; set; }

    // Constructor που αρχικοποιεί τον πίνακα Sudoku
    public Sudoku(int[,] board)
    {
        Board = board;
    }

    // Εκτυπώνει τον πίνακα Sudoku
    public void PrintBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Console.Write(Board[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
