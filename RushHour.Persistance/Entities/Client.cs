using System.ComponentModel.DataAnnotations;

namespace RushHour.Persistance.Entities
{
    public class Client : BaseEntity
    {
        [Required]
        public string Phone { get; set; }

        [Required]
        [MinLength(3)]
        public string Address { get; set; }

        [Required]
        public virtual Account Account { get; set; }
    }
}
