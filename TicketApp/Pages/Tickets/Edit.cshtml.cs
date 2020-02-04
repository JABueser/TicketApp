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
    [Authorize(Roles = "Administrators")]
    public class EditModel : DI_BasePageModel
    {
        private readonly ApplicationDbContext _db;

        public EditModel(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager) : base(context, authorizationService, userManager)
        {
            _db = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string returnUrl = null)
        {
            ReturnUrl = Request.Headers["Referer"].ToString();
            Ticket = await _db.Tickets.FindAsync(id);
            if (Ticket == null)
            {
                return RedirectToPage("../Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _db.Attach(Ticket).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new Exception($"Ticket {Ticket.Title} is being updated.", e);
            }
            return Redirect(returnUrl);
        }
    }
}