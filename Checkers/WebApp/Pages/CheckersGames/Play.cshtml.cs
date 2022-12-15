using System.Diagnostics;
using DAL;
using DAL.Db;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebApp.Pages_CheckersGames;

public class Play : PageModel
{
    public readonly IGameRepository _repo;

    public Play(IGameRepository repo)
    {
        _repo = repo;
    }

    public CheckersBrain Brain { get; set; } = default!;
    public CheckersGame CheckersGame { get; set; } = default!;

    public int PlayerNo { get; set; }

    public async Task<IActionResult> OnGet(int? id, int? playerNo, int? x, int? y, bool? checkAi, bool? undoMove)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new { error = "No game id!" });
        }

        if (playerNo == null || playerNo.Value < 0 || playerNo.Value > 1)
        {
            return RedirectToPage("/Index", new { error = "No player no, or wrong no!" });
        }

        PlayerNo = playerNo.Value;
        
        // playerNo 0 - first player. First player is always red.
        // playerNo 1 - second player. Second player is always black.
        
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
        } else if (checkAi.HasValue && (
           playerNo == 0 && CheckersGame.Player2Type == EPlayerType.Ai ||
           playerNo == 1 && CheckersGame.Player1Type == EPlayerType.Ai))
        {
            Brain.MakeAMoveByAi();
            game.CheckersGameStates!.Add(new CheckersGameState()
            {
                SerializedGameState = Brain.GetSerializedGameState()
            });

            _repo.SaveChanges();
        }

        return Page();
    }
}