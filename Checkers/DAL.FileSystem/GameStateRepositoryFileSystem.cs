using Domain;

namespace DAL.FileSystem;

public class GameStateRepositoryFileSystem : ICurrentGameStateOptionsRepository
{
    private const string FileExtension = "json";
    private readonly string _statesDirectory = "." + Path.DirectorySeparatorChar + "states";
    
    public string Name { get; } = "FileSystem";
    public List<string> GetPreviousGameStatesList()
    {
        CheckOrCreateDirectory();
        
        var res = new List<string>();

        foreach (var fileName in Directory.GetFileSystemEntries(_statesDirectory, "*." + FileExtension))
        {
            res.Add(Path.GetFileNameWithoutExtension(fileName));
        }
        return res;
    }

    public CheckersGameState GetGameState(string id)
    {
        var fileContent = File.ReadAllText(GetFileName(id));
        var state = System.Text.Json.JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (state == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }

        return state;
    }

    public void SaveCurrentGameState(string id, CheckersGameState state)
    {
        CheckOrCreateDirectory();
        
        var fileContent = System.Text.Json.JsonSerializer.Serialize(state);
        File.WriteAllText(GetFileName(id), fileContent);
    }

    public void DeleteGameState(string id)
    {
        File.Delete(GetFileName(id));
    }
    
    private string GetFileName(string id)
    {
        return _statesDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
    }

    private void CheckOrCreateDirectory()
    {
        if (!Directory.Exists(_statesDirectory))
        {
            Directory.CreateDirectory(_statesDirectory);
        }
    }
}