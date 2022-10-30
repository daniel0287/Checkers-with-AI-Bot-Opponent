using Domain;

namespace DAL;

public interface ICurrentGameStateOptionsRepository
{
    // crud methods

    //read
    List<string> GetPreviousGameStatesList();
    CheckersGameState GetGameState(string id);
    
    // create and update
    void SaveCurrentGameState(string id, CheckersGameState state);
    
    // delete
    void DeleteGameState(string id);
}