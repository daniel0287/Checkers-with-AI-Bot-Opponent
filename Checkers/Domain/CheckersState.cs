namespace Domain;

public class CheckersState
{
    public EGamePiece?[][] GameBoard = default!;
    public bool NextMoveByBlack { get; set; }
}