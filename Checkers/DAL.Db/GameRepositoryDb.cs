using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameRepositoryDb : BaseRepository, IGameRepository
{
    public GameRepositoryDb(AppDbContext dbContext) : base(dbContext)
    {
    }
    
    public List<CheckersGame> getAll()
    {
        return Ctx.CheckersGames
            .Include(c => c.CheckersOption)
            .OrderBy(o => o.StartedAt)
            .ToList();
    }

    public CheckersGame? GetGame(int? id)
    {
        return Ctx.CheckersGames
            .Include(g => g.CheckersOption)
            .Include(g => g.CheckersGameStates)
            .FirstOrDefault(g => g.Id == id);
    }

    public CheckersGame AddGame(CheckersGame game)
    {
        Ctx.CheckersGames.Add(game);
        Ctx.SaveChanges();

        return game;
    }
}