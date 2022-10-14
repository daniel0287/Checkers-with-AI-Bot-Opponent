using System.Text;
using ConsoleUI;
using DAL;
using DAL.FileSystem;
using Domain;
using GameBrain;
using static System.Console;

namespace MenuSystem;

public class Game
{
    private readonly IGameOptionsRepository _gameOptionsRepo = new GameOptionsRepositoryFileSystem();
    private readonly ICurrentGameStateOptionsRepository _gameStatesRepo = new GameStateRepositoryFileSystem();
    private CheckersOptions _gameOptions = new CheckersOptions();
    private CheckersGameState _gameState = new CheckersGameState();
    private CheckersBrain _game = new CheckersBrain(new CheckersOptions());
    private bool _back;
    public void Start()
    {
        if (!File.Exists("Default Options.json"))
        {
           _gameOptionsRepo.SaveGameOptions("Default Options", new CheckersOptions());
        }
        Title = "Checkers";
        RunMainMenu();
    }

    private void RunMainMenu()
    {
        do
        {
            _back = false;
            string prompt = "   Checkers\n=================";
            string[] choices = { "New Game", "Save Game", "Load Game", "Options", "Exit" };
            Menu mainMenu = new Menu(prompt, choices);
            int selectedIndex = mainMenu.Run();
            string outcome = ""; // If empty nothing happens, if back, back was pressed.
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

    private void ExitGame()
    {
        Environment.Exit(0);
    }
    
    private string RunNewGame()
    {
        Clear();
        if (_gameOptions.Width == 0)
        {
            _gameOptions.Width = 8;
        }
        if (_gameOptions.Height == 0)
        {
            _gameOptions.Height = 8;
        }
        _game = new CheckersBrain(_gameOptions);
        Ui.DrawGameBoard(_game.GetBoard());
        return "";
    }

    private string SaveGame()
    {
        Clear();
        Console.WriteLine("   Save Game\n=====================");
        Console.Write("Game name:");
        var gameStateName = Console.ReadLine();
        if (gameStateName != null) _gameStatesRepo.SaveCurrentGameState(gameStateName, _gameState);
        return "back";
    }
    
    private string LoadGame()
    {
        string prompt = "   Load Game\n=================";
        string[] choices = new string[_gameStatesRepo.GetPreviousGameStatesList().Count + 2];
        int index = 0;
        foreach (var state in _gameStatesRepo.GetPreviousGameStatesList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Delete Load Game";
        choices[index + 1] = "Back";
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
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
        string prompt = "Delete Load Game\n=================";
        string[] choices = new string[_gameStatesRepo.GetPreviousGameStatesList().Count + 1];
        int index = 0;
        foreach (var state in _gameStatesRepo.GetPreviousGameStatesList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var stateName = choices[selectedIndex];
        _gameStatesRepo.DeleteGameStates(stateName);
        return "back";
    }
    
    private string RunOptions()
    {
        do
        {
            _back = false;
            string prompt = "   Options\n=================";
            string[] choices =
            {
                "Create options", "List saved options", "Load options", "Delete options", "Save current options",
                "Edit current options", "Back"
            };
            Menu optionsMenu = new Menu(prompt, choices);
            int selectedIndex = optionsMenu.Run();
            string outcome = "";
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
                    return "back";
            }
            if (outcome == "back")
            {
                _back = true;
            }
        } while (_back);
        return "";
    }
    
    /*private void CustomBoard()
    {
        Clear();
        Console.WriteLine("Board width?");
        Console.WriteLine("-------------------");
        Console.Write("Your choice:");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Width can't be empty");
        }
        int width = int.Parse(boardChoice);
        
        Clear();
        Console.WriteLine("Board height (must be an even number)");
        Console.WriteLine("-------------------");
        Console.Write("Your choice:");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Height can't be empty;");
        }
        int height = int.Parse(boardChoice);
        Clear();
        var options = _gameOptions;
        options.Width = width;
        options.Height = height;
        _game = new CheckersBrain(options);
        Ui.DrawGameBoard(_game.GetBoard());
    } */

    string CreateGameOptions()
    {
        Clear();
        Console.WriteLine("  Create Options\n=====================");
        Console.Write("Board width:");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Width can't be empty");
        }
        int width = int.Parse(boardChoice);
        Console.Write("Board height (must be an even number):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Height can't be empty;");
        }
        int height = int.Parse(boardChoice);
        CheckersOptions newGameOptions = new CheckersOptions(); 
        newGameOptions.Width = width;
        newGameOptions.Height = height;
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        if (optionsName != null) _gameOptionsRepo.SaveGameOptions(optionsName, _gameOptions);
        return "back";
    }
    
    string ListGameOptions()
    {
        StringBuilder outcome = new StringBuilder();
        foreach (var name in _gameOptionsRepo.GetGameOptionsList())
        {
            outcome.Append(name + "\n");
        }
        string prompt = outcome.ToString();
        string[] choices =
        {
            "Back"
        };
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        switch (selectedIndex)
        {
            case 0:
                return "back";
        }
        return "";
    }

    string LoadGameOptions()
    {
        string prompt = "  Load Options\n=================";
        string[] choices = new string[_gameOptionsRepo.GetGameOptionsList().Count + 1];
        int index = 0;
        foreach (var state in _gameOptionsRepo.GetGameOptionsList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var optionsName = choices[selectedIndex];
        _gameOptions = _gameOptionsRepo.GetGameOptions(optionsName);
        return "back";
    }

    string DeleteGameOptions()
    {
        string prompt = "   Delete Game Options\n=================";
        string[] choices = new string[_gameOptionsRepo.GetGameOptionsList().Count + 1];
        int index = 0;
        foreach (var state in _gameOptionsRepo.GetGameOptionsList())
        {
            choices[index] = state;
            index += 1;
        }
        choices[index] = "Back";
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        var optionsName = choices[selectedIndex];
        _gameOptionsRepo.DeleteGameOptions(optionsName);
        return "back";
    }

    string SaveCurrentGameOptions()
    {
        Clear();
        Console.WriteLine("Save Current Options\n=====================");
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        if (optionsName != null) _gameOptionsRepo.SaveGameOptions(optionsName, _gameOptions);
        return "back";
    }

    string EditCurrentGameOptions()
    {
        Clear();
        Console.WriteLine("Edit Current Options\n=====================");
        Console.Write("Board width:");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Width can't be empty");
        }
        int width = int.Parse(boardChoice);
        Console.Write("Board height (must be an even number):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "")
        {
            throw new ArgumentException("Height can't be empty;");
        }
        int height = int.Parse(boardChoice);
        Clear();
        _gameOptions.Width = width;
        _gameOptions.Height = height;
        return "back";
    }
    
}