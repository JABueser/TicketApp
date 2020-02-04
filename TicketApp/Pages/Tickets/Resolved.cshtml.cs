using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketApp.Authorization;
using TicketApp.Data;
using TicketApp.Models;

namespace TicketApp
{
    public class ResolvedModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ResolvedModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public string TitleSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }

        public PaginatedList<Ticket> Tickets { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, string dateStart, string dateEnd, int? pageIndex)
        {
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

            IQueryable<Ticket> ticketsIQ = from t in _db.Tickets
                                           where t.Resolved.Equals(true)
                                           select t;
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!String.IsNullOrEmpty(dateStart) && !String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime start = DateTime.ParseExact(dateStart, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    DateTime end = DateTime.ParseExact(dateEnd, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t => start <= t.ResolvedOn && t.ResolvedOn <= end);

                }
                catch (FormatException e)
                {

                }

            }
            else if (!String.IsNullOrEmpty(dateStart) && String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime start = DateTime.ParseExact(dateStart, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t => start <= t.ResolvedOn);
                }
                catch
                {

                }
            }
            else if (String.IsNullOrEmpty(dateStart) && !String.IsNullOrEmpty(dateEnd))
            {
                try
                {
                    DateTime end = DateTime.ParseExact(dateEnd, new string[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, provider, DateTimeStyles.None);
                    ticketsIQ = ticketsIQ.Where(t => t.ResolvedOn <= end);
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
                    ticketsIQ = ticketsIQ.OrderBy(t => t.ResolvedOn);
                    break;
                case "date_desc":
                    ticketsIQ = ticketsIQ.OrderByDescending(t => t.ResolvedOn);
                    break;
                default:
                    ticketsIQ = ticketsIQ.OrderBy(t => t.Title);
                    break;
            }

            int pageSize = 10;
            Tickets = await PaginatedList<Ticket>.CreateAsync(ticketsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var isAuthorized = User.IsInRole(Constants.AdministratorsRole);

            if (isAuthorized)
            {
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    _db.Tickets.Remove(ticket);
                    await _db.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }
    }
}