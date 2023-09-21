using RushHour.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RushHour.Persistance.Entities
{
    public class Account : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        public byte[] Salt { get; set; }

        [Required]
        public Roles Role { get; set; }

        [Required]
        public string Username { get; set; }
    }
}
