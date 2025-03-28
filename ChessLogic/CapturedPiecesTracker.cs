namespace ChessLogic;

public class CapturedPiecesTracker
{
    private List<Piece> capturedByWhite = [];
    private List<Piece> capturedByBlack = [];

    private int materialAdvantage = 0;

    public event Action<List<Piece>, List<Piece>, int> OnTrackerUpdated;

    private static readonly Dictionary<PieceType, int> pieceValues = new Dictionary<PieceType, int>
    {
        { PieceType.Pawn, 1 },
        { PieceType.Knight, 3 },
        { PieceType.Bishop, 3 },
        { PieceType.Rook, 5 },
        { PieceType.Queen, 9 },
        { PieceType.King, 0 }
    };

    public CapturedPiecesTracker()
    {
        Reset();
    }

    public void AddCapturedPiece(Piece capturedPiece, Player capturingPlayer)
    {
        if (capturedPiece == null)
            return;

        if (capturingPlayer == Player.White)
        {
            capturedByWhite.Add(capturedPiece);
            materialAdvantage += pieceValues[capturedPiece.Type];
        }
        else if (capturingPlayer == Player.Black)
        {
            capturedByBlack.Add(capturedPiece);
            materialAdvantage -= pieceValues[capturedPiece.Type];
        }

        OnTrackerUpdated?.Invoke(capturedByWhite, capturedByBlack, materialAdvantage);
    }

    public void Reset()
    {
        capturedByWhite.Clear();
        capturedByBlack.Clear();
        materialAdvantage = 0;
    }
}
