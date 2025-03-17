namespace ChessLogic;

public class GameState
{
    public Board Board { get;  }
    public Player CurrentPlayer { get; private set; }
    public Result Result { get; private set; } = null;

    private int noCaptureOrPawnMoves = 0;
    private string stateString;

    public ChessTimer Timer { get; private set; }
    public CapturedPiecesTracker CapturedPieces { get; private set; }

    public bool TimerEnabled { get; set; }

    private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();

    public GameState(Player player, Board board)
    {
        CurrentPlayer = player;
        Board = board;

        stateString = new StateString(CurrentPlayer, board).ToString();
        stateHistory[stateString] = 1;

        Timer = new ChessTimer(600);
        CapturedPieces = new CapturedPiecesTracker();
        TimerEnabled = false;
    }

    public GameState(Player player, Board board, int timeInSeconds, int incrementInSeconds = 0)
            : this(player, board)
    {
        Timer = new ChessTimer(timeInSeconds, incrementInSeconds);
        TimerEnabled = true;
    }

    public IEnumerable<Move> LegalMovesForPiece(Position pos)
    {
        if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
        {
            return Enumerable.Empty<Move>();
        }

        Piece piece = Board[pos];
        IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    public void MakeMove(Move move)
    {
        Piece capturedPiece = null;
        if (!Board.IsEmpty(move.ToPos) && Board[move.ToPos].Color != CurrentPlayer)
        {
            capturedPiece = Board[move.ToPos];
        }

        if (capturedPiece != null)
        {
            CapturedPieces.AddCapturedPiece(capturedPiece, CurrentPlayer);
        }

        Board.SetPawnSkipPosition(CurrentPlayer, null);
        bool captureOrPawn = move.Execute(Board);

        if (captureOrPawn)
        {
            noCaptureOrPawnMoves = 0;
            stateHistory.Clear();
        } 
        else
        {
            noCaptureOrPawnMoves++;
        }

        CurrentPlayer = CurrentPlayer.Opponent();

        if (TimerEnabled)
        {
            Timer.SwitchClock();
        }

        UpdateStateString();
        CheckForGameOver();
    }

    public IEnumerable<Move> AllLegalMovesFor(Player player)
    {
        IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
        {
            Piece piece = Board[pos];
            return piece.GetMoves(pos, Board);
        });

        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    private void CheckForGameOver()
    {
        if (TimerEnabled)
        {
            Player timeoutPlayer;
            if (Timer.IsTimeOut(out timeoutPlayer))
            {
                Result = Result.Win(timeoutPlayer.Opponent(), EndReason.TimeOut);
                return;
            }
        }

        if (!AllLegalMovesFor(CurrentPlayer).Any())
        {
            if (Board.IsInCheck(CurrentPlayer))
            {
                Result = Result.Win(CurrentPlayer.Opponent(), EndReason.Checkmate);
            }
            else
            {
                Result = Result.Draw(EndReason.Stalemate);
            }
        }
        else if (Board.InsufficientMaterial())
        {
            Result = Result.Draw(EndReason.InsufficientMaterial);
        }
        else if (FiftyMoveRule())
        {
            Result = Result.Draw(EndReason.FiftyMoveRule);
        }
        else if (ThreefoldRepetition())
        {
            Result = Result.Draw(EndReason.ThreefoldRepetition);
        }
    }

    public bool IsGameOver()
    {
        return Result != null;
    }

    private bool FiftyMoveRule()
    {
        int fullMoves = noCaptureOrPawnMoves / 2;
        return fullMoves == 50;
    }

    private void UpdateStateString()
    {
        stateString = new StateString(CurrentPlayer, Board).ToString();

        if (!stateHistory.ContainsKey(stateString))
        {
            stateHistory[stateString] = 1;
        }
        else
        {
            stateHistory[stateString]++;
        }
    }

    private bool ThreefoldRepetition()
    {
        return stateHistory[stateString] == 3;
    }

    public void StartGame()
    {
        if (TimerEnabled)
        {
            Timer.Start(CurrentPlayer);
        }
    }

    public void PauseGame()
    {
        if (TimerEnabled)
        {
            Timer.Pause();
        }
    }

    public void ResumeGame()
    {
        if (TimerEnabled)
        {
            Timer.Resume();
        }
    }

    public void SetTimeControl(int timeInSeconds, int incrementInSeconds = 0)
    {
        Timer = new ChessTimer(timeInSeconds, incrementInSeconds);
        TimerEnabled = true;
    }
}
