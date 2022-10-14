namespace Domain;

public class CheckersOptions
{
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;
    public int RandomMoves { get; set; }
    public bool WhiteStarts { get; set; } = true;

    public override string ToString()
    {
        return $"Board: {Width}x{Height} Random: {RandomMoves} WhiteStarts: {WhiteStarts}";
    }
}