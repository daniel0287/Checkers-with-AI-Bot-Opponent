using Domain;

namespace DAL;

public interface IGameRepository : IBaseRepository
{
    List<CheckersGame> GetAll();
    CheckersGame? GetGame(int? id);
    CheckersGame AddGame(CheckersGame game);
}