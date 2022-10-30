using Domain;

namespace DAL.Db;

public class GameStateRepositoryDb : ICurrentGameStateOptionsRepository
{
    private readonly AppDbContext _dbContext;
    
    public GameStateRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public string Name { get; } = "DB";
    
    public List<string> GetPreviousGameStatesList() =>
        _dbContext
            .CheckersGameStates
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();

    public CheckersGameState GetGameState(string id)
    {
        return _dbContext
            .CheckersGameStates
            .First(o => o.Name == id);
    }

    public void SaveCurrentGameState(string id, CheckersGameState state)
    {
        var statesFromDb = _dbContext
            .CheckersGameStates
            .FirstOrDefault(o => o.Name == id);
        if (statesFromDb == null)
        {
            _dbContext.CheckersGameStates.Add(state);
            _dbContext.SaveChanges();
            return;
        }

        statesFromDb.Name = state.Name;
        statesFromDb.CreatedAt = state.CreatedAt;

        _dbContext.SaveChanges();
    }

    public void DeleteGameState(string id)
    {
        var stateFromDb = GetGameState(id);
        _dbContext.CheckersGameStates.Remove(stateFromDb);
        _dbContext.SaveChanges();
    }
}