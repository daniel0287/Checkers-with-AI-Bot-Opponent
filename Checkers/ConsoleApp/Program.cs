// See https://aka.ms/new-console-template for more information

using MenuSystem;

namespace ConsoleApp;

internal static class Program
{
    private static void Main()
    {
        var myGame = new Game();
        myGame.Start();
    }
}