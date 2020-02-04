using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketApp.Authorization;
using TicketApp.Data;
using TicketApp.Models;
using TicketApp.Pages.Tickets;

namespace TicketApp.Pages
{
    public class IndexModel : DI_BasePageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager) : base(context, authorizationService, userManager)
        {
            _db = context;
        }


        //public IList<Ticket> Tickets { get; private set; }
        public string TitleSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        [DisplayName("Show Critical Only")]
        public bool ShowCritical { get; set; }
        public string DateStart { get; set; }
        public string DateEnd{ get; set; }

        public PaginatedList<Ticket> Tickets { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, bool showCritical, string searchString, string dateStart, string dateEnd, int? pageIndex)
        {
            ShowCritical = showCritical;
            CurrentSort = sortOrder;
            TitleSort = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            DateStart = dateStart;
            DateEnd = dateEnd;

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            CurrentFilter = searchString;

            
            IQueryable<Ticket> ticketsIQ = from t in _db.Tickets where t.Resolved.Equals(false)
                                             select t;

            if (ShowCritical.Equals(true))
            {
                ticketsIQ = ticketsIQ.Where(t => t.Critical.Equals(true));
            }

            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!String.IsNullOrEmpty(dateStart) && !String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime start = DateTime.ParseExact(dateStart, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    DateTime end = DateTime.ParseExact(dateEnd, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t =>  start <= t.DateCreated && t.DateCreated <= end);
    
                }
                catch (FormatException e)
                {
                    
                }

            }else if(!String.IsNullOrEmpty(dateStart) && String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime start = DateTime.ParseExact(dateStart, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t => start <= t.DateCreated);
                }
                catch
                {

                }
            }else if(String.IsNullOrEmpty(dateStart) && !String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime end = DateTime.ParseExact(dateEnd, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t => t.DateCreated <= end);
                }
                catch
                {

                }
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                ticketsIQ = ticketsIQ.Where(t => t.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    ticketsIQ = ticketsIQ.OrderByDescending(t => t.Title);
                    break;
                case "Date":
                    ticketsIQ = ticketsIQ.OrderBy(t => t.DateCreated);
                    break;
                case "date_desc":
                    ticketsIQ = ticketsIQ.OrderByDescending(t => t.DateCreated);
                    break;
                default:
                    ticketsIQ = ticketsIQ.OrderBy(t => t.Title);
                    break;
            }

            int pageSize = 10;
            Tickets = await PaginatedList<Ticket>.CreateAsync(ticketsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostResolveAsync(int id)
        {
            var isAuthorized = User.IsInRole(Constants.AdministratorsRole);

            if (isAuthorized)
            {
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    ticket.ResolvedOn = DateTime.UtcNow;
                    ticket.Resolved = true;
                    _db.Attach(ticket).State = EntityState.Modified;
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        throw new Exception($"Ticket {ticket.Title} is being updated.", e);
                    }
                }
            }
            return RedirectToPage();
        }
    }
}
