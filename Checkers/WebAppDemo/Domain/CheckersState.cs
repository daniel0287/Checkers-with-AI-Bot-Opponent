namespace WebAppDemo.Domain;

public class CheckersState
{
    public int Id { get; set; }
    public EGamePiece?[][] GameBoard = default!;
    public bool NextMoveByBlack { get; set; }
    public ICollection<CheckersGameState>? CheckersGameStates { get; set; }
}