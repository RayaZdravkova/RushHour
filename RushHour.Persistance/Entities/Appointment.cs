using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHour.Persistance.Entities
{
    public class Appointment : BaseEntity
    {

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public virtual Employee Employee { get; set; }

        [Required]
        public virtual Client Client { get; set; }

        [Required]
        public virtual Activity Activity { get; set; }
    }
}
