using static System.Console;

namespace MenuSystem;

public class Menu
{
    private int _selectedIndex;
    private readonly string[] _choices;
    private readonly string _prompt;

    public Menu(string prompt, string[] choices)
    {
        _prompt = prompt;
        _choices = choices;
        _selectedIndex = 0;
    }

    private void DisplayOptions()
    {
        WriteLine(_prompt);
        for (int i = 0; i < _choices.Length; i++)
        {
            string currentOption = _choices[i];
            string prefix;

            if (i == _selectedIndex)
            {
                prefix = "*";
                ForegroundColor = ConsoleColor.Black;
                BackgroundColor = ConsoleColor.White;
            }
            else
            {
                prefix = " ";
                ForegroundColor = ConsoleColor.White;
                BackgroundColor = ConsoleColor.Black;
            }

            WriteLine($"{prefix} << {currentOption} >>");
        }

        ResetColor();
    }

    public int Run()
    {
        ConsoleKey keyPressed;
        do
        {
            Clear();
            DisplayOptions();

            ConsoleKeyInfo keyInfo = ReadKey(true);
            keyPressed = keyInfo.Key;

            // Update SelectedIndex based on arrow keys.
            if (keyPressed == ConsoleKey.UpArrow)
            {
                _selectedIndex--;
                if (_selectedIndex == -1)
                {
                    _selectedIndex = _choices.Length - 1;
                }
            }
            else if (keyPressed == ConsoleKey.DownArrow)
            {
                _selectedIndex++;
                if (_selectedIndex == _choices.Length)
                {
                    _selectedIndex = 0;
                }
            }
        } while (keyPressed != ConsoleKey.Enter);

        return _selectedIndex;
    }
}