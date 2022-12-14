namespace Domain;

public class CheckersOption
{
    // PK in db
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;

    // ICollection - no foo[]
    public ICollection<CheckersGame>? CheckersGames { get; set; }
    
    public override string ToString()
    {
        return $"Board: {Width}x{Height}";
    }
}