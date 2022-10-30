using Domain;

namespace DAL.Db;

public class GameOptionsRepositoryDb : IGameOptionsRepository
{
    private readonly AppDbContext _dbContext;
    
    public GameOptionsRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public string Name { get; } = "DB";
    
    public List<string> GetGameOptionsList() =>
        _dbContext
            .CheckersOptions
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();

    public CheckersOption GetGameOptions(string id)
    {
        return _dbContext
            .CheckersOptions
            .First(o => o.Name == id);
    }

    public void SaveGameOptions(string id, CheckersOption option)
    {
        var optionsFromDb = _dbContext
            .CheckersOptions
            .FirstOrDefault(o => o.Name == id);
        if (optionsFromDb == null)
        {
            _dbContext.CheckersOptions.Add(option);
            _dbContext.SaveChanges();
            return;
        }

        optionsFromDb.Name = option.Name;
        optionsFromDb.Width = option.Width;
        optionsFromDb.Height = option.Height;
        optionsFromDb.RandomMoves = option.RandomMoves;
        optionsFromDb.WhiteStarts = option.WhiteStarts;

        _dbContext.SaveChanges();
    }

    public void DeleteGameOptions(string id)
    {
        var optionsFromDb = GetGameOptions(id);
        _dbContext.CheckersOptions.Remove(optionsFromDb);
        _dbContext.SaveChanges();
    }
}