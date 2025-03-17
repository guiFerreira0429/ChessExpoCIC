using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic;

public class CapturedPiecesTracker
{
    private List<Piece> capturedByWhite = new List<Piece>();
    private List<Piece> capturedByBlack = new List<Piece>();

    private int whiteMaterialAdvantage = 0;

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
            whiteMaterialAdvantage += pieceValues[capturedPiece.Type];
        }
        else if (capturingPlayer == Player.Black)
        {
            capturedByBlack.Add(capturedPiece);
            whiteMaterialAdvantage -= pieceValues[capturedPiece.Type];
        }
    }

    public void Reset()
    {
        capturedByWhite.Clear();
        capturedByBlack.Clear();
        whiteMaterialAdvantage = 0;
    }

    public IEnumerable<Piece> GetCapturedPieces(Player player)
    {
        return player == Player.White ? capturedByWhite : capturedByBlack;
    }

    public int GetMaterialAdvantage()
    {
        return whiteMaterialAdvantage;
    }

    public string GetFormattedAdvantage()
    {
        if (whiteMaterialAdvantage == 0)
            return "+0";

        if (whiteMaterialAdvantage > 0)
            return $"+{whiteMaterialAdvantage}";
        else
            return $"{whiteMaterialAdvantage}";
    }
}
