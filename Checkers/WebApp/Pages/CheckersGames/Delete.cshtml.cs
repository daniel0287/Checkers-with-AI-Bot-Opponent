using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_CheckersGames
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public CheckersGame CheckersGame { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.CheckersGames == null)
            {
                return NotFound();
            }

            var checkersgame = await _context.CheckersGames.FirstOrDefaultAsync(m => m.Id == id);

            if (checkersgame == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersGame = checkersgame;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.CheckersGames == null)
            {
                return NotFound();
            }
            var checkersgame = await _context.CheckersGames.FindAsync(id);

            if (checkersgame != null)
            {
                CheckersGame = checkersgame;
                _context.CheckersGames.Remove(CheckersGame);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
