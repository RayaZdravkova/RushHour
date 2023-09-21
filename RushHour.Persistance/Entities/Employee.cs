using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHour.Persistance.Entities
{
    public class Employee : BaseEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal RatePerHour { get; set; }

        [Required]  
        public DateTime HireDate { get; set; }

        [Required]
        public virtual Provider Provider { get; set; }

        [Required]
        public virtual Account Account { get; set; }
    }
}
