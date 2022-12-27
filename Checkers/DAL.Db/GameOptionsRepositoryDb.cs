using Domain;

namespace DAL.Db;

public class GameOptionsRepositoryDb : BaseRepository, IGameOptionsRepository
{
    public GameOptionsRepositoryDb(AppDbContext dbContext) : base(dbContext)
    {
    }
    
    public List<string> GetGameOptionsList() =>
        Ctx
            .CheckersOptions
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();

    public CheckersOption GetGameOptions(string name)
    {
        return Ctx
            .CheckersOptions
            .First(o => o.Name == name);
    }

    public void SaveGameOptions(string id, CheckersOption option)
    {
        var optionsFromDb = Ctx
            .CheckersOptions
            .FirstOrDefault(o => o.Name == id);
        if (optionsFromDb == null)
        {
            Ctx.CheckersOptions.Add(option);
            Ctx.SaveChanges();
            return;
        }

        optionsFromDb.Name = option.Name;
        optionsFromDb.Width = option.Width;
        optionsFromDb.Height = option.Height;

        Ctx.SaveChanges();
    }

    public void DeleteGameOptions(string id)
    {
        var optionsFromDb = GetGameOptions(id);
        Ctx.CheckersOptions.Remove(optionsFromDb);
        Ctx.SaveChanges();
    }
}