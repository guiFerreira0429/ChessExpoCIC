namespace ChessLogic;

public enum EndReason
{
    None,
    Checkmate,
    Stalemate,
    FiftyMoveRule,
    InsufficientMaterial,
    ThreefoldRepetition,
    TimeOut
}
