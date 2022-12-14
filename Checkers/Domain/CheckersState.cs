namespace Domain;

public class CheckersState
{
    public EGameTileState?[][] GameBoard { get; set; } = default!;
    public bool NextMoveByBlack { get; set; }
}