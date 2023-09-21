using RushHour.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RushHour.Persistance.Entities
{
    public class Provider : BaseEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public string Website { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string BusinessDomain { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public DateTime StartTimeOfTheWorkingDay { get; set; }

        [Required]
        public DateTime EndTimeOfTheWorkingDay { get; set; }

        [Required]
        public DaysOfWeek WorkingDays { get; set; }
    }
}
