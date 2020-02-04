using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketApp.Authorization;
using TicketApp.Data;
using TicketApp.Models;
using TicketApp.Pages.Tickets;

namespace TicketApp
{
    public class CreateModel : DI_BasePageModel
    {
        private readonly ApplicationDbContext _db;

        public CreateModel(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager) : base(context, authorizationService, userManager)
        {
            _db = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Ticket, TicketingOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            Ticket.Resolved = false;
            _db.Tickets.Add(Ticket);
            await Context.SaveChangesAsync();

            return RedirectToPage("../Index");
        }
    }
}