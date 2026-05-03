using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace api.data.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string? ImageUrl { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}