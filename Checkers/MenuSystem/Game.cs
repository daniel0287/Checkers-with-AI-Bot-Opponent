using ConsoleUI;
using DAL;
using DAL.FileSystem;
using Domain;
using GameBrain;
using static System.Console;

namespace MenuSystem;

public class Game
{
    private readonly IGameOptionsRepository _repo = new GameOptionsRepositoryFileSystem();
    private CheckersOptions _gameOptions = new CheckersOptions();
    private CheckersBrain _game = new CheckersBrain(new CheckersOptions());
    private bool _back;
    public void Start()
    {
        Title = "Checkers";
        RunMainMenu();
    }

    private void RunMainMenu()
    {
        do
        {
            _back = false;
            string prompt = "   Checkers\n=================";
            string[] choices = { "New Game", "Load Game", "Options", "Exit" };
            Menu mainMenu = new Menu(prompt, choices);
            int selectedIndex = mainMenu.Run();
            string outcome = ""; // If empty nothing happens, if back, back was pressed.
            switch (selectedIndex)
            {
                case 0:
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

    private void ExitGame()
    {
        Environment.Exit(0);
    }

    private string RunOptions()
    {
        string prompt = "   Options\n=================";
        string[] choices =
        {
            "Create options", "List saved options", "Load options", "Delete options", "Save current options",
            "Edit current options", "Back"
        };
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        switch (selectedIndex)
        {
            case 0:
                break;
            case 1:
                foreach (var name in _repo.GetGameOptionsList())
                {
                    Console.WriteLine(name);
                }
                break;
            case 2:
                Console.Write("Options name:");
                var optionsName = Console.ReadLine();
                if (optionsName != null) _gameOptions = _repo.GetGameOptions(optionsName);
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                return "back";
        }
        return "";
    }

    private string LoadGame()
    {
        string prompt = "   Load Game\n=================";
        string[] choices = { "Back" };
        Menu optionsMenu = new Menu(prompt, choices);
        int selectedIndex = optionsMenu.Run();
        switch (selectedIndex)
        {
            case 0:
                return "back";
        }
        return "";
    }

    private string RunNewGame()
    {
        string prompt = "What board size would you like?\n=================";
        string[] choices = { "Standard 8x8", "Custom", "Back" };
        Menu boardSizeMenu = new Menu(prompt, choices);
        int selectedIndex = boardSizeMenu.Run();

        switch (selectedIndex)
        {
            case 0:
                var standardOptions = _gameOptions;
                standardOptions.Width = 8;
                standardOptions.Height = 8;
                _game = new CheckersBrain(standardOptions);
                Clear();
                Ui.DrawGameBoard(_game.GetBoard());
                break;
            case 1:
                CustomBoard();
                break;
            case 2:
                return "back";
        }
        return "";
    }

    private void CustomBoard()
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
    }
}