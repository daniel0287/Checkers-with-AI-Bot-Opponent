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
    private static readonly IGameOptionsRepository GameOptionsRepoDb = new GameOptionsRepositoryDb(Ctx);
    private IGameOptionsRepository _gameOptionsRepo = GameOptionsRepoDb;
    private static CheckersOption _gameOption = new CheckersOption();
    private CheckersGameState _gameState = new CheckersGameState();
    private CheckersBrain _game = new CheckersBrain(new CheckersOption(), null);
    private readonly IGameRepository _gameRepoDb = new GameRepositoryDb(Ctx);
    private int _currentGameId;

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
            string[] choices = { "New Game", "Load Game", "Options", "Exit" };
            var mainMenu = new Menu(prompt, choices);
            var selectedIndex = mainMenu.Run();
            var outcome = ""; // If empty nothing happens, if back, back was pressed.
            switch (selectedIndex)
            {
                case 0:
                    Clear();
                    outcome = RunNewGame();
                    break;
                case 1:
                    outcome = LoadGame();
                    break;
                case 2:
                    outcome = RunOptions();
                    break;
                case 3:
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
        Console.WriteLine("   New Game\n=================");
        var newGame = new CheckersGame();
        Console.Write("Player 1 name:");
        var player1Name = Console.ReadLine();
        if (player1Name == "")
        {
            Clear();
            Console.WriteLine("Name can not be empty!");
            return RunNewGame();
        }
        newGame.Player1Name = player1Name!;
        Console.Write("Player 1 type (human/ai):");
        var player1Type = Console.ReadLine();
        if (player1Type == "human")
        {
            newGame.Player1Type = EPlayerType.Human;
        }
        else if (player1Type == "ai")
        {
            newGame.Player1Type = EPlayerType.Ai;
        }
        else
        {
            Clear();
            Console.WriteLine("Invalid input. Please enter 'human' or 'ai'.");
            return RunNewGame();
        }
        Console.Write("Player 2 name:");
        var player2Name = Console.ReadLine();
        if (player2Name == "")
        {
            Clear();
            Console.WriteLine("Name can not be empty!");
            return RunNewGame();
        }
        newGame.Player2Name = player2Name!;
        Console.Write("Player 2 type (human/ai):");
        var player2Type = Console.ReadLine();
        if (player2Type == "human")
        {
            newGame.Player2Type = EPlayerType.Human;
        }
        else if (player2Type == "ai")
        {
            newGame.Player2Type = EPlayerType.Ai;
        }
        else
        {
            Clear();
            Console.WriteLine("Invalid input. Please enter 'human' or 'ai'.");
            return RunNewGame();
        }

        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        if (optionsName == null || !_gameOptionsRepo.GetGameOptionsList().Contains(optionsName))
        {
            Clear();
            Console.WriteLine("Invalid input. Please enter a saved option.");
            return RunNewGame();
        }
        newGame.CheckersOption = _gameOptionsRepo.GetGameOptions(optionsName);
        _gameRepoDb.AddGame(newGame);
        _gameOption = _gameOptionsRepo.GetGameOptions(optionsName);
        _game = new CheckersBrain(_gameOption, null);
        _currentGameId = newGame.Id;
        return RunGame();
    }
    
    private string RunGame()
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
        return MakeATurn();
    }

    private string MakeATurn()
    {
        var selectedX = 0;
        var selectedY = 0;
        var colLength = _game.GetBoard().Length;
        var rowLength = _game.GetBoard()[0].Length;
        Clear();
        Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
        Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
        while (true)
        {
            if (_gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates == null || _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Count == 0)
            {
                _game = new CheckersBrain(_gameOption, null);
            }
            else 
            {
                if (_gameState != _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Last())
                {
                    _gameState = _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Last();
                    _game = new CheckersBrain(_gameOption, _gameState);
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                
            }
            if (GameOver())
            {
                _gameRepoDb.GetGame(_currentGameId)!.GameWonByPlayer = _game.NextMoveByBlack() ? _gameRepoDb.GetGame(_currentGameId)!.Player1Name : _gameRepoDb.GetGame(_currentGameId)!.Player2Name;
                _gameRepoDb.GetGame(_currentGameId)!.GameOverAt = DateTime.Now;
                _gameRepoDb.SaveChanges();
                GameOverScreen(_game.NextMoveByBlack() ? "red" : "blue");
                break;
            }
            // Check if a key has been pressed
            if (_gameRepoDb.GetGame(_currentGameId)!.Player2Type == EPlayerType.Ai && _game.NextMoveByBlack())
            {
                _game.MakeAMoveByAi();
                _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Add(new CheckersGameState()
                {
                    SerializedGameState = _game.GetSerializedGameState()
                });

                _gameRepoDb.SaveChanges();
                _gameState = _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Last();
                _game = new CheckersBrain(_gameOption, _gameState);
                Thread.Sleep(1000);
                Clear();
                Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
            }
            else if (_gameRepoDb.GetGame(_currentGameId)!.Player1Type == EPlayerType.Ai && !_game.NextMoveByBlack())
            {
                _game.MakeAMoveByAi();
                _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Add(new CheckersGameState()
                {
                    SerializedGameState = _game.GetSerializedGameState()
                });

                _gameRepoDb.SaveChanges();
                _gameState = _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Last();
                _game = new CheckersBrain(_gameOption, _gameState);
                Thread.Sleep(1000);
                Clear();
                Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
            }
            if (Console.KeyAvailable)
            {
                // Read the key that was pressed
                ConsoleKeyInfo key = Console.ReadKey(true);

                // Update the selected piece based on the key that was pressed
                if (key.Key == ConsoleKey.UpArrow && selectedY > 0)
                {
                    selectedY--;
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                else if (key.Key == ConsoleKey.DownArrow && selectedY < rowLength - 1)
                {
                    selectedY++;
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                else if (key.Key == ConsoleKey.LeftArrow && selectedX > 0)
                {
                    selectedX--;
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                else if (key.Key == ConsoleKey.RightArrow && selectedX < colLength - 1)
                {
                    selectedX++;
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                else if (key.Key == ConsoleKey.Enter)
                { 
                    _game.MakeAMove(selectedX, selectedY);
                    _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Add(new CheckersGameState()
                    {
                        SerializedGameState = _game.GetSerializedGameState()
                    });

                    _gameRepoDb.SaveChanges();
                    Clear();
                    Console.WriteLine("Current turn: " + (_game.NextMoveByBlack() ? "blue" : "red"));
                    Ui.DrawGameBoard(_game.GetBoard(), selectedX, selectedY);
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    return "back";
                }
            }
        }

        return "";
    }
    
    /*private string SaveGame()
    {
        Clear();
        Console.WriteLine("   Save Game\n=====================");
        Console.Write("Game name:");
        var gameStateName = Console.ReadLine();
        _gameState.Name = gameStateName!;
        _gameState.SerializedGameState = System.Text.Json.JsonSerializer.Serialize(_gameState);
        if (gameStateName != null) _gameStatesRepo.SaveCurrentGameState(gameStateName, _gameState);
        return "back";
    }*/

    private bool GameOver()
    {
        for (int x = 0; x < _game.GetBoard().Length; x++)
        {
            for (int y = 0; y < _game.GetBoard()[0].Length; y++)
            {
                if (_game.GetBoard()[x][y] == EGameTileState.ForcedMove || _game.GetBoard()[x][y] == EGameTileState.PossibleMove)
                {
                    return false;
                }
            }
        }
        
        if (!_game.ExistenceOfForcedJumps())
        {
            for (int x = 0; x < _game.GetBoard().Length; x++)
            {
                int diff = x % 2 == 0 ? 1 : 0;

                for (int y = diff; y < _game.GetBoard()[0].Length; y += 2)
                {
                    if (_game.JumpsAvailableWithoutForced())
                    {
                        return false;
                    }
                    
                    if (_game.PieceHasMoves(x, y))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    private void GameOverScreen(string winner)
    {
        string prompt = "Game Over, " + winner + " victory!\n============================";
        string[] choices = { "New Game", "Load Game", "Options", "Exit" };
        var mainMenu = new Menu(prompt, choices);
        var selectedIndex = mainMenu.Run();
        switch (selectedIndex)
        {
            case 0: 
                RunNewGame();
                break;
            case 1:
                LoadGame();
                break;
            case 2:
                RunOptions();
                break;
            case 3:
                ExitGame();
                break;
        }
    }
    
    private string LoadGame()
    {
        const string prompt = "   Load Game\n=================";
        var choices = new string[_gameRepoDb.GetAll().Count + 2];
        var index = 0;
        foreach (var game in _gameRepoDb.GetAll())
        {
            choices[index] = game.ToString();
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
        // _gameState = _gameStatesRepo.GetGameState(choices[selectedIndex]);
        _currentGameId = _gameRepoDb.GetAll()[selectedIndex].Id;
        _gameOption = _gameRepoDb.GetGame(_currentGameId)!.CheckersOption!;
        if (_gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates == null || _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Count == 0)
        {
            _game = new CheckersBrain(_gameOption, null);
        }
        else
        {
            _gameState = _gameRepoDb.GetGame(_currentGameId)!.CheckersGameStates!.Last();
            _game = new CheckersBrain(_gameOption, _gameState);
        }
        return RunGame();
    }

    private string DeleteLoadGame()
    {
        const string prompt = "Delete Load Game\n=================";
        var choices = new string[_gameRepoDb.GetAll().Count + 1];
        var index = 0;
        foreach (var game in _gameRepoDb.GetAll())
        {
            choices[index] = game.ToString();
            index += 1;
        }
        choices[index] = "Back";
        var optionsMenu = new Menu(prompt, choices);
        var selectedIndex = optionsMenu.Run();
        if (selectedIndex == index)
        {
            return "back";
        }
        _gameRepoDb.DeleteGame(_gameRepoDb.GetAll()[selectedIndex].Id);
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
                    Clear();
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
                    Clear();
                    outcome = SaveCurrentGameOptions();
                    break;
                case 5:
                    Clear();
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
        Console.WriteLine("  Create Options\n=====================");
        Console.Write("Board width (even number and at least 4):");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "" || int.Parse(boardChoice) % 2 != 0 || int.Parse(boardChoice) < 4)
        {
            Clear();
            Console.WriteLine("Invalid input!");
            return CreateGameOptions();
        }
        var width = int.Parse(boardChoice);
        Console.Write("Board height (even number and at least 4):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "" || int.Parse(boardChoice) % 2 != 0 || int.Parse(boardChoice) < 4)
        {
            Clear();
            Console.WriteLine("Invalid input!");
            return CreateGameOptions();
        }
        var height = int.Parse(boardChoice);
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        if (optionsName == "")
        {
            Clear();
            Console.WriteLine("Name can not be empty!");
            return CreateGameOptions();
        }
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
        Console.WriteLine("Save Current Options\n=====================");
        Console.Write("Options name:");
        var optionsName = Console.ReadLine();
        if (optionsName == "")
        {
            Clear();
            Console.WriteLine("Name can not be empty!");
            return SaveCurrentGameOptions();
        }
        _gameOption.Name = optionsName!;
        if (optionsName != null) _gameOptionsRepo.SaveGameOptions(optionsName, _gameOption);
        return "back";
    }

    private static string EditCurrentGameOptions()
    {
        Console.WriteLine("Edit Current Options\n=====================");
        Console.Write("Board width (even number and at least 4):");
        var boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "" || int.Parse(boardChoice) % 2 != 0 || int.Parse(boardChoice) < 4)
        {
            Clear();
            Console.WriteLine("Invalid input!");
            return EditCurrentGameOptions();
        }
        var width = int.Parse(boardChoice);
        Console.Write("Board height (even number and at least 4):");
        boardChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";
        if (boardChoice == "" || int.Parse(boardChoice) % 2 != 0 || int.Parse(boardChoice) < 4)
        {
            Clear();
            Console.WriteLine("Invalid input!");
            return EditCurrentGameOptions();
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

        _gameOptionsRepo = optionsName == "FileSystem" ? _gameOptionsRepoFs : GameOptionsRepoDb;
        return "back";
    }
    
}