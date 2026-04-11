using System.Globalization;

const char Human = 'X';
const char Computer = 'O';

Console.WriteLine("Tic Tac Toe");
Console.WriteLine($"{Human} = You, {Computer} = Computer");
Console.WriteLine();

var rng = new Random();

while (true)
{
    var board = new char[9];

    while (true)
    {
        RenderBoard(board);

        var humanMove = ReadHumanMoveOrQuit(board);
        if (humanMove is null)
        {
            Console.WriteLine("Goodbye.");
            return;
        }

        board[humanMove.Value - 1] = Human;

        if (HasWinner(board, Human))
        {
            RenderBoard(board);
            Console.WriteLine("You win!");
            break;
        }

        if (IsDraw(board))
        {
            RenderBoard(board);
            Console.WriteLine("Draw.");
            break;
        }

        var availableMoves = GetAvailableMoves(board);
        var computerMove = ChooseComputerMove(rng, availableMoves);
        board[computerMove - 1] = Computer;
        Console.WriteLine($"Computer chose: {computerMove}");

        if (HasWinner(board, Computer))
        {
            RenderBoard(board);
            Console.WriteLine("Computer wins.");
            break;
        }

        if (IsDraw(board))
        {
            RenderBoard(board);
            Console.WriteLine("Draw.");
            break;
        }
    }

    if (!PromptPlayAgain())
    {
        Console.WriteLine("Goodbye.");
        return;
    }
}

static void RenderBoard(char[] board)
{
    Console.WriteLine();

    for (var row = 0; row < 3; row++)
    {
        var i0 = row * 3;
        Console.Write(" ");
        Console.Write(CellText(board, i0));
        Console.Write(" | ");
        Console.Write(CellText(board, i0 + 1));
        Console.Write(" | ");
        Console.Write(CellText(board, i0 + 2));
        Console.WriteLine();

        if (row < 2)
        {
            Console.WriteLine("---+---+---");
        }
    }

    Console.WriteLine();
}

static string CellText(char[] board, int index)
{
    var c = board[index];
    return c == '\0' ? (index + 1).ToString(CultureInfo.InvariantCulture) : c.ToString();
}

static bool HasWinner(char[] board, char player)
{
    int[,] lines =
    {
        { 0, 1, 2 },
        { 3, 4, 5 },
        { 6, 7, 8 },
        { 0, 3, 6 },
        { 1, 4, 7 },
        { 2, 5, 8 },
        { 0, 4, 8 },
        { 2, 4, 6 },
    };

    for (var i = 0; i < lines.GetLength(0); i++)
    {
        if (board[lines[i, 0]] == player && board[lines[i, 1]] == player && board[lines[i, 2]] == player)
        {
            return true;
        }
    }

    return false;
}

static bool IsDraw(char[] board)
{
    foreach (var c in board)
    {
        if (c == '\0')
        {
            return false;
        }
    }

    return true;
}

static List<int> GetAvailableMoves(char[] board)
{
    var moves = new List<int>(capacity: 9);

    for (var i = 0; i < board.Length; i++)
    {
        if (board[i] == '\0')
        {
            moves.Add(i + 1);
        }
    }

    return moves;
}

static int? ReadHumanMoveOrQuit(char[] board)
{
    while (true)
    {
        Console.Write("Your move (1-9, q to quit): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            continue;
        }

        if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var move))
        {
            continue;
        }

        if (move is < 1 or > 9)
        {
            continue;
        }

        if (board[move - 1] != '\0')
        {
            continue;
        }

        return move;
    }
}

static int ChooseComputerMove(Random rng, List<int> moves)
{
    return moves[rng.Next(moves.Count)];
}

static bool PromptPlayAgain()
{
    while (true)
    {
        Console.Write("Play again? (y/n): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            continue;
        }

        if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
    }
}
