using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_CheckersGames
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository _repo;

        public IndexModel(IGameRepository repo)
        {
            _repo = repo;
        }

        public IList<CheckersGame> CheckersGame { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CheckersGame = _repo.GetAll();
        }
    }
}
