using Domain;

namespace DAL;

public interface IGameRepository : IBaseRepository
{
    List<CheckersGame> getAll();
    CheckersGame? GetGame(int? id);
    CheckersGame AddGame(CheckersGame game);
}