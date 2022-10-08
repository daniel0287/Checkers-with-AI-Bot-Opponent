namespace Domain;

public class CheckersGameState
{
    public EGamePiece?[,] GameBoard = default!;
    public bool NextMoveByBlack = true;
}