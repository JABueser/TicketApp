using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketApp.Models
{
    public class Ticket
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool Resolved { get; set; }
        [Required]
        public bool Critical { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime DateCreated { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime ResolvedOn { get; set; }

        public Ticket()
        {
            this.DateCreated = DateTime.UtcNow;
        }

        public string DescriptionTrimmed
        {
            get
            {
                if (Description.Length > 20)
                {
                    return Description.Substring(0, 30) + "...";
                }
                else
                {
                    return Description;
                }
            }
        }
    }
}
