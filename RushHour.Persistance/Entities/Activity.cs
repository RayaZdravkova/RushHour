using System.ComponentModel.DataAnnotations;

namespace RushHour.Persistance.Entities
{
    public class Activity : BaseEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        [Required]
        public virtual Provider Provider { get; set; }
    }
}
