using System.ComponentModel.DataAnnotations;

namespace RushHour.Persistance.Entities
{
    public class ActivityEmployee
    {
        public int EmployeeId { get; set; }
        public int ActivityId { get; set; }

        [Required]
        public virtual Employee Employee { get; set; }

        [Required]
        public virtual Activity Activity { get; set; }
    }
}
