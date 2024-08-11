namespace SudokuGame;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class Solver
{
    public static void Main(string[] args)
    {
        // Έλεγχος εάν ο αριθμός των ορισμάτων είναι σωστός
        if (args.Length == 0)
        {
            var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();

            Console.WriteLine($"SudokuGame v{versionString}");
            Console.WriteLine("-------------");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  SudokuGame <filename> <datastructure>");

            Console.WriteLine("\nData structure options:");
            Console.WriteLine("  1. ArrayList Stack");
            Console.WriteLine("  2. ArrayList Queue");
            Console.WriteLine("  3. Stack");
            Console.WriteLine("  4. LinkedList Queue");
            return;
        }

        if (args.Length != 2)
        {
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  SudokuGame <filename> <datastructure>");
            return;
        }

        // Διαβάζουμε το όνομα του αρχείου και τον αριθμό της δομής δεδομένων από τα ορίσματα
        string fileName = args[0];
        int dataStructure = int.Parse(args[1]);

        // Διαβάζουμε τον αρχικό πίνακα από το αρχείο
        int[,] initialBoard;
        try
        {
            initialBoard = ReadBoardFromFile(fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        Sudoku sudoku = new Sudoku(initialBoard);
        
        // Επιλογή της μεθόδου επίλυσης ανάλογα με τη δομή δεδομένων
        switch (dataStructure)
        {
            case 1:
                SolvePuzzleUsingArrayListStack(sudoku);
                break;
            case 2:
                SolvePuzzleUsingArrayListQueue(sudoku);
                break;
            case 3:
                SolvePuzzleUsingStack(sudoku);
                break;
            case 4:
                SolvePuzzleUsingLinkedListQueue(sudoku);
                break;
            default:
                Console.WriteLine("Invalid data structure choice.");
                return;
        }
    }

    /// <summary>
    /// Ελέγχει εάν η κίνηση είναι έγκυρη σύμφωνα με τους κανόνες του παιχνιδιού.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    /// <param name="row">Η σειρά του κελιού.</param>
    /// <param name="col">Η στήλη του κελιού.</param>
    /// <param name="num">Ο αριθμός που επιχειρείται να τοποθετηθεί.</param>
    /// <returns>
    ///     <c>true</c> αν η κίνηση είναι έγκυρη, αλλιώς <c>false</c>.
    /// </returns>
    private static bool IsValidMove(Sudoku sudoku, int row, int col, int num)
    {
        // Έλεγχος αν ο αριθμός δεν υπάρχει ήδη στην ίδια σειρά ή στην ίδια στήλη
        for (int i = 0; i < 9; i++)
        {
            if (sudoku.Board[row, i] == num || sudoku.Board[i, col] == num)
                return false;
        }

        // Έλεγχος αν ο αριθμός δεν υπάρχει ήδη στο τετράγωνο 3x3
        int boxStartRow = row - row % 3;
        int boxStartCol = col - col % 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (sudoku.Board[boxStartRow + i, boxStartCol + j] == num)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Βρίσκει την πρώτη κενή θέση (κελί με τιμή 0) στον πίνακα Sudoku.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    /// <param name="row">Ο δείκτης της γραμμής της κενής θέσης.</param>
    /// <param name="col">Ο δείκτης της στήλης της κενής θέσης.</param>
    /// <returns>
    ///   <c>true</c> αν βρεθεί μια κενή θέση, αλλιώς <c>false</c> αν ο πίνακας είναι πλήρως συμπληρωμένος.
    /// </returns>
    private static bool FindEmptyLocation(Sudoku sudoku, out int row, out int col)
    {
        // Διατρέχει κάθε γραμμή και στήλη για να βρει την πρώτη κενή θέση (τιμή 0).
        for (row = 0; row < 9; row++)
        {
            for (col = 0; col < 9; col++)
            {
                if (sudoku.Board[row, col] == 0)
                    return true;
            }
        }

        // Αν δεν βρεθεί κενή θέση, ορίζει τους δείκτες row και col σε -1 και επιστρέφει false.
        row = -1;
        col = -1;
        return false;
    }

    /// <summary>
    /// Λύνει το Sudoku χρησιμοποιώντας αναδρομικά τον αλγόριθμο επίλυσης.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    /// <returns>
    ///   <c>true</c> αν βρεθεί λύση για το Sudoku, αλλιώς <c>false</c> αν δεν υπάρχει λύση.
    /// </returns>
    public static bool SolveSudoku(Sudoku sudoku)
    {
        int row, col;

        // Εάν δεν υπάρχει κενή θέση, όλα τα κελιά είναι συμπληρωμένα και η λύση είναι βραβείο.
        if (!FindEmptyLocation(sudoku, out row, out col))
            return true;

        // Δοκιμάζουμε αναδρομικά κάθε δυνατή τιμή για το κελί.
        for (int num = 1; num <= 9; num++)
        {
            if (IsValidMove(sudoku, row, col, num))
            {
                // Τοποθετούμε τον αριθμό στο κελί.
                sudoku.Board[row, col] = num;

                // Αν η τρέχουσα επιλογή οδηγεί σε λύση, επιστρέφουμε true.
                if (SolveSudoku(sudoku))
                    return true;

                // Αν η τρέχουσα επιλογή δεν οδηγεί σε λύση, επαναφέρουμε το κελί σε 0.
                sudoku.Board[row, col] = 0;
            }
        }

        // Δεν υπάρχει λύση για την τρέχουσα κατάσταση του Sudoku.
        return false;
    }

    /// <summary>
    /// Διαβάζει το Board από ένα αρχείο και δημιουργεί τον αντίστοιχο πίνακα.
    /// </summary>
    /// <param name="fileName">Το όνομα του αρχείου που περιέχει το Board.</param>
    /// <returns>Πίνακας 9x9 με τους αριθμούς του Board.</returns>
    private static int[,] ReadBoardFromFile(string fileName)
    {
        int[,] board = new int[9, 9];

        if (!File.Exists(fileName))
        {
            throw new Exception($"Error: File '{fileName}' does not exist.");
        }

        try
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                for (int i = 0; i < 9; i++)
                {
                    // Διαβάζει μια γραμμή από το αρχείο.
                    string line = sr.ReadLine()!;

                    if (line != null)
                    {
                        // Χωρίζει τα νούμερα χρησιμοποιώντας το κενό ως διαχωριστικό.
                        string[] numbers = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int j = 0; j < 9; j++)
                        {
                            // Μετατρέπει τα strings σε ακέραιους και τα εισάγει στον πίνακα.
                            if (int.TryParse(numbers[j], out int parsedNumber))
                            {
                                board[i, j] = parsedNumber;
                            }
                            else
                            {
                                Console.WriteLine($"Error parsing number at row {i}, column {j}");
                            }
                        }
                    }
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading from file: " + e.Message);
        }

        return board;
    }

    #region Μέθοδοι επίλυσης

    /// <summary>
    /// Λύνει το Sudoku χρησιμοποιώντας στοίβα υλοποιημένη με ArrayList.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    private static void SolvePuzzleUsingArrayListStack(Sudoku sudoku)
    {
        // Εκτύπωση του αρχικού πίνακα
        Console.WriteLine("Initial Sudoku Board:");
        sudoku.PrintBoard();

        Stack<ArrayList> stack = new Stack<ArrayList>();
        ArrayList initialConfig = new ArrayList { sudoku };
        stack.Push(initialConfig);

        while (stack.Count > 0)
        {
            ArrayList currentConfig = stack.Pop();
            Sudoku currentSudoku = (Sudoku)currentConfig[currentConfig.Count - 1]!;

            int row, col;
            if (!FindEmptyLocation(currentSudoku, out row, out col))
            {
                // Εάν δεν υπάρχει κενή θέση, έχουμε βρει τη λύση.
                Console.WriteLine("Sudoku Solved using ArrayList Stack:");
                currentSudoku.PrintBoard();
                return;
            }

            // Δοκιμάζουμε κάθε δυνατή τιμή για το κελί.
            for (int num = 1; num <= 9; num++)
            {
                if (IsValidMove(currentSudoku, row, col, num))
                {
                    // Δημιουργούμε ένα νέο αντικείμενο Sudoku με τον αριθμό στο κελί.
                    Sudoku newSudoku = new Sudoku((int[,])currentSudoku.Board.Clone());
                    newSudoku.Board[row, col] = num;

                    // Δημιουργούμε ένα νέο σύνολο παραμέτρων με τον νέο πίνακα Sudoku.
                    ArrayList newConfig = new ArrayList(currentConfig);
                    newConfig.Add(newSudoku);

                    // Προσθέτουμε το νέο σύνολο στη στοίβα.
                    stack.Push(newConfig);
                }
            }
        }

        Console.WriteLine("No solution found using ArrayList Stack.");
    }

    /// <summary>
    /// Λύνει το Sudoku χρησιμοποιώντας ουρά υλοποιημένη με ArrayList.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    private static void SolvePuzzleUsingArrayListQueue(Sudoku sudoku)
    {
        // Εκτύπωση του αρχικού πίνακα
        Console.WriteLine("Initial Sudoku Board:");
        sudoku.PrintBoard();

        Queue<ArrayList> queue = new Queue<ArrayList>();
        ArrayList initialConfig = new ArrayList { sudoku };
        queue.Enqueue(initialConfig);

        while (queue.Count > 0)
        {
            ArrayList currentConfig = queue.Dequeue();
            Sudoku currentSudoku = (Sudoku)currentConfig[currentConfig.Count - 1]!;

            int row, col;
            if (!FindEmptyLocation(currentSudoku, out row, out col))
            {
                // Εάν δεν υπάρχει κενή θέση, έχουμε βρει τη λύση.
                Console.WriteLine("Sudoku Solved using ArrayList Queue:");
                currentSudoku.PrintBoard();
                return;
            }

            // Δοκιμάζουμε κάθε δυνατή τιμή για το κελί.
            for (int num = 1; num <= 9; num++)
            {
                if (IsValidMove(currentSudoku, row, col, num))
                {
                    // Δημιουργούμε ένα νέο αντικείμενο Sudoku με τον αριθμό στο κελί.
                    Sudoku newSudoku = new Sudoku((int[,])currentSudoku.Board.Clone());
                    newSudoku.Board[row, col] = num;

                    // Δημιουργούμε ένα νέο σύνολο παραμέτρων με τον νέο πίνακα Sudoku.
                    ArrayList newConfig = new ArrayList(currentConfig);
                    newConfig.Add(newSudoku);

                    // Προσθέτουμε το νέο σύνολο στην ουρά.
                    queue.Enqueue(newConfig);
                }
            }
        }

        Console.WriteLine("No solution found using ArrayList Queue.");
    }

    /// <summary>
    /// Λύνει το Sudoku χρησιμοποιώντας στοίβα υλοποιημένη με Stack.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    private static void SolvePuzzleUsingStack(Sudoku sudoku)
    {
        // Εκτύπωση του αρχικού πίνακα
        Console.WriteLine("Initial Sudoku Board:");
        sudoku.PrintBoard();

        Stack<Sudoku> stack = new Stack<Sudoku>();
        stack.Push(sudoku);

        while (stack.Count > 0)
        {
            Sudoku currentSudoku = stack.Pop();

            int row, col;
            if (!FindEmptyLocation(currentSudoku, out row, out col))
            {
                // Εάν δεν υπάρχει κενή θέση, έχουμε βρει τη λύση.
                Console.WriteLine("Sudoku Solved using Stack:");
                currentSudoku.PrintBoard();
                return;
            }

            // Δοκιμάζουμε κάθε δυνατή τιμή για το κελί.
            for (int num = 1; num <= 9; num++)
            {
                if (IsValidMove(currentSudoku, row, col, num))
                {
                    // Δημιουργούμε ένα νέο αντικείμενο Sudoku με τον αριθμό στο κελί.
                    Sudoku newSudoku = new Sudoku((int[,])currentSudoku.Board.Clone());
                    newSudoku.Board[row, col] = num;

                    // Προσθέτουμε το νέο αντικείμενο στη στοίβα.
                    stack.Push(newSudoku);
                }
            }
        }

        // Εάν φτάσουμε εδώ, δεν βρέθηκε λύση.
        Console.WriteLine("No solution found using Stack.");
    }

    /// <summary>
    /// Λύνει το Sudoku χρησιμοποιώντας ουρά υλοποιημένη με LinkedList.
    /// </summary>
    /// <param name="sudoku">Το αντικείμενο Sudoku που περιέχει το Board.</param>
    private static void SolvePuzzleUsingLinkedListQueue(Sudoku sudoku)
    {
        // Εκτύπωση του αρχικού πίνακα
        Console.WriteLine("Initial Sudoku Board:");
        sudoku.PrintBoard();

        Queue<LinkedList<Sudoku>> queue = new Queue<LinkedList<Sudoku>>();
        LinkedList<Sudoku> initialConfig = new LinkedList<Sudoku>();
        initialConfig.AddLast(sudoku);
        queue.Enqueue(initialConfig);

        while (queue.Count > 0)
        {
            LinkedList<Sudoku> currentConfig = queue.Dequeue();
            Sudoku currentSudoku = currentConfig.Last!.Value;

            int row, col;
            if (!FindEmptyLocation(currentSudoku, out row, out col))
            {
                // Εάν δεν υπάρχει κενή θέση, έχουμε βρει τη λύση.
                Console.WriteLine("Sudoku Solved using LinkedList Queue:");
                currentSudoku.PrintBoard();
                return;
            }

            // Δοκιμάζουμε κάθε δυνατή τιμή για το κελί.
            for (int num = 1; num <= 9; num++)
            {
                if (IsValidMove(currentSudoku, row, col, num))
                {
                    // Δημιουργούμε ένα νέο αντικείμενο Sudoku με τον αριθμό στο κελί.
                    Sudoku newSudoku = new Sudoku((int[,])currentSudoku.Board.Clone());
                    newSudoku.Board[row, col] = num;

                    // Δημιουργούμε ένα νέο σύνολο παραμέτρων με τον νέο πίνακα Sudoku.
                    LinkedList<Sudoku> newConfig = new LinkedList<Sudoku>(currentConfig);
                    newConfig.AddLast(newSudoku);

                    // Προσθέτουμε το νέο σύνολο στην ουρά.
                    queue.Enqueue(newConfig);
                }
            }
        }

        Console.WriteLine("No solution found using LinkedList Queue.");
    }

    #endregion
}