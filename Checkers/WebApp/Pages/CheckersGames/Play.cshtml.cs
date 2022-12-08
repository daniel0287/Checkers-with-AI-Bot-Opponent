using DAL;
using DAL.Db;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages_CheckersGames;

public class Play : PageModel
{
    private readonly IGameRepository _repo;

    public Play(IGameRepository repo)
    {
        _repo = repo;
    }

    public CheckersBrain Brain { get; set; } = default!;
    public CheckersGame CheckersGame { get; set; } = default!;

    public async Task<IActionResult> OnGet(int? id, int? x, int? y)
    {
        var game = _repo.GetGame(id);

        if (game == null || game.CheckersOption == null)
        {
            return NotFound();
        }

        CheckersGame = game;

        Brain = new CheckersBrain(game.CheckersOption, game.CheckersGameStates?.LastOrDefault());
        
        if (x != null && y != null)
        {
            Brain.MakeAMove(x.Value, y.Value);
            game.CheckersGameStates!.Add(new CheckersGameState()
           {
               SerializedGameState = Brain.GetSerializedGameState()
           });

           _repo.SaveChanges();
        }

        return Page();
    }
}