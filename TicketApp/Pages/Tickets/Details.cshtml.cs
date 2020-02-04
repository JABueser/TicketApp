using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketApp.Data;
using TicketApp.Models;
using TicketApp.Pages.Tickets;

namespace TicketApp
{
    public class DetailsModel : DI_BasePageModel
    {
        private readonly ApplicationDbContext _db;

        public DetailsModel(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager) : base(context, authorizationService, userManager)
        {
            _db = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Ticket = await _db.Tickets.FindAsync(id);
            if (Ticket == null)
            {
                return RedirectToPage("../Index");
            }
            return Page();
        }
    }
}