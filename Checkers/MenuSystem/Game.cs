using System.Text;
using ConsoleUI;
using DAL;
using DAL.Db;
using DAL.FileSystem;
using Domain;
using GameBrain;
using Microsoft.EntityFrameworkCore;
using static System.Console;

namespace MenuSystem;



public class Game
{
    private static readonly DbContextOptions<AppDbContext> DbOptions =
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=/Users/danie/RiderProjects/icd0008-2022f/Checkers/app.db")
            .Options;
    private static readonly AppDbContext Ctx = new AppDbContext(DbOptions);
    private readonly IGameOptionsRepository _gameOptionsRepoFs = new GameOptionsRepositoryFileSystem();
    private readonly ICurrentGameStateOptionsRepository _gameStatesRepoFs = new GameStateRepositoryFileSystem();
    private static readonly IGameOptionsRepository GameOptionsRepoDb = new GameOptionsRepositoryDb(Ctx);
    private static readonly ICurrentGameStateOptionsRepository GameStatesRepoDb = new GameStateRepositoryDb(Ctx);
    private IGameOptionsRepository _gameOptionsRepo = GameOptionsRepoDb;
    private ICurrentGameStateOptionsRepository _gameStatesRepo = GameStatesRepoDb;
    private static CheckersOption _gameOption = new CheckersOption();
    private CheckersGameState _gameState = new CheckersGameState();
    private CheckersBrain _game = new CheckersBrain(new CheckersOption(), null);
    private bool _back;
    public void Start()
    {
        _gameOption.Name = "Default Options";
        GameOptionsRepoDb.SaveGameOptions(_gameOption.Name, _gameOption);
        if (!File.Exists("Default Options.json"))
        {
           _gameOptionsRepoFs.SaveGameOptions("Default Options", new CheckersOption());
        }
        Title = "Checkers";
        RunMainMenu();
    }

    private void RunMainMenu()
    {
        do
        {
            _back = false;
            const string prompt = "   Checkers\n=================";
            string[] choices = { "New Game", "Save Game", "Load Game", "Options", "Exit" };
            var mainMenu = new Menu(prompt, choices);
            var selectedIndex = mainMenu.Run();
            var outcome = ""; // If empty nothing happens, if back, back was pressed.
            switch (selectedIndex)
            {
                case 0:
                    outcome = RunNewGame();
                    break;
                case 1:
                    outcome = SaveGame();
                    break;
                case 2:
                    outcome = LoadGame();
                    break;
                case 3:
                    outcome = RunOptions();
                    break;
                case 4:
                    ExitGame();
                    break;
            }
            if (outcome == "back")
            {
                _back = true;
            }
        } while (_back);
    }

    private static void ExitGame()
    {
        Environment.Exit(0);
    }
    
    private string RunNewGame()
    {
        Clear();
        if (_gameOption.Width == 0)
        {
            _gameOption.Width = 8;
        }
        if (_gameOption.Height == 0)
        {
            _gameOption.Height = 8;
        }
        _game = new CheckersBrain(_gameOption, null);
        Ui.DrawGameBoard(_game.GetBoard());
        return "";
    }

    private string SaveGame()
    {
        Clear();
        Console.WriteLine("   Save Game\n=====================");
        Console.Write("Game name:");
        var gameStateName = Console.ReadLine();
        _gameState.Name = gameStateName!;
        _gameState.SerializedGameState = System.Text.Json.JsonSerializer.Serialize(_gameState);
        if (gameStateName != null) _gameStatesRepo.SaveCurrentGameState(gameStateName, _gameState);
        return "back";
    }

    private string LoadGame()
    {
        const string prompt = "   Load Game\n=================";
        var choices = new string[_gameStatesRepo.GetPreviousGameStatesList().Count + 2];
        var index = 0;
        foreach (var state in _gameStatesRepo.GetPreviousGameStatesList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Delete Load Game";
        choices[index + 1] = "Back";
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return DeleteLoadGame();
        }
        if (selectedIndex == index + 1)
        {
            return "back";
        }
        _gameState = _gameStatesRepo.GetGameState(choices[selectedIndex]);
        RunNewGame();
        return "";
    }

    private string DeleteLoadGame()
    {
        const string prompt = "Delete Load Game\n=================";
        var choices = new string[_gameStatesRepo.GetPreviousGameStatesList().Count + 1];
        var index = 0;
        foreach (var state in _gameStatesRepo.GetPreviousGameStatesList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var stateName = choices[selectedIndex];
        _gameStatesRepo.DeleteGameState(stateName);
        return "back";
    }
    
    private string RunOptions()
    {
        do
        {
            _back = false;
            const string prompt = "   Options\n=================";
            string[] choices =
            {
                "Create options", "List saved options", "Load options", "Delete options", "Save current options",
                "Edit current options", "Persistence method swap", "Back"
            };
            var optionsMenu = new Menu(prompt, choices);
            var selectedIndex = optionsMenu.Run();
            var outcome = "";
            switch (selectedIndex)
            {
                case 0:
                    outcome = CreateGameOptions();
                    break;
                case 1:
                    outcome = ListGameOptions();
                    break;
                case 2:
                    outcome = LoadGameOptions();
                    break;
                case 3:
                    outcome = DeleteGameOptions();
                    break;
                case 4:
                    outcome = SaveCurrentGameOptions();
                    break;
                case 5:
                    outcome = EditCurrentGameOptions();
                    break;
                case 6:
                    outcome = SwapPersistenceEngine();
                    break;
                case 7:
                    return "back";
            }
            if (outcome == "back")
            {
                _back = true;
            }
        } while (_back);
        return "";
    }

    private string CreateGameOptions()
    {
        Clear();
        Console.WriteLine("  Create Options\n=====================");
        Console.Write("Board width:");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Width can't be empty");
        }
        var width = int.Parse(boardChoice);
        Console.Write("Board height (must be an even number):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Height can't be empty;");
        }
        var height = int.Parse(boardChoice);
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        var newGameOption = new CheckersOption
        {
            Width = width,
            Height = height,
            Name = optionsName!
        };
        
        if (optionsName != null) _gameOptionsRepo.SaveGameOptions(optionsName, newGameOption);
        return "back";
    }

    private string ListGameOptions()
    {
        var outcome = new StringBuilder();
        foreach (var name in _gameOptionsRepo.GetGameOptionsList())
        {
            outcome.Append(name + "\n");
        }
        var prompt = outcome.ToString();
        string[] choices =
        {
            "Back"
        };
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        return selectedIndex switch
        {
            0 => "back",
            _ => ""
        };
    }

    private string LoadGameOptions()
    {
        const string prompt = "  Load Options\n=================";
        var choices = new string[_gameOptionsRepo.GetGameOptionsList().Count + 1];
        var index = 0;
        foreach (var state in _gameOptionsRepo.GetGameOptionsList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var optionsName = choices[selectedIndex];
        _gameOption = _gameOptionsRepo.GetGameOptions(optionsName);
        return "back";
    }

    private string DeleteGameOptions()
    {
        const string prompt = "   Delete Game Options\n=================";
        var choices = new string[_gameOptionsRepo.GetGameOptionsList().Count + 1];
        var index = 0;
        foreach (var state in _gameOptionsRepo.GetGameOptionsList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var optionsName = choices[selectedIndex];
        _gameOptionsRepo.DeleteGameOptions(optionsName);
        return "back";
    }

    private string SaveCurrentGameOptions()
    {
        Clear();
        Console.WriteLine("Save Current Options\n=====================");
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        _gameOption.Name = optionsName!;
        if (optionsName != null) _gameOptionsRepo.SaveGameOptions(optionsName, _gameOption);
        return "back";
    }

    private static string EditCurrentGameOptions()
    {
        Clear();
        Console.WriteLine("Edit Current Options\n=====================");
        Console.Write("Board width:");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Width can't be empty");
        }
        var width = int.Parse(boardChoice);
        Console.Write("Board height (must be an even number):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Height can't be empty;");
        }
        var height = int.Parse(boardChoice);
        Clear();
        _gameOption.Width = width;
        _gameOption.Height = height;
        return "back";
    }

    private string SwapPersistenceEngine()
    {
        var currentEngine = _gameOptionsRepo == GameOptionsRepoDb ? "Database" : "FileSystem";
        var prompt = "Swap Persistence Engine\nCurrent Engine: " + currentEngine + "\n========================";
        string[] choices = { "FileSystem", "DataBase", "back" };
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == 2)
        {
            return "back";
        }
        var optionsName = choices[selectedIndex];

        if (optionsName == "FileSystem")
        {
            _gameOptionsRepo = _gameOptionsRepoFs;
            _gameStatesRepo = _gameStatesRepoFs;
        }
        else
        {
            _gameOptionsRepo = GameOptionsRepoDb;
            _gameStatesRepo = GameStatesRepoDb;
        }
        return "back";
    }
    
}