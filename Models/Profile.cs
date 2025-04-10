using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITAssetManagement1.Models
{
    public class Profile
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        // Add a foreign key to link Profile to IdentityUser
        public string UserId { get; set; } // Foreign key to IdentityUser
        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; } // Navigation property

        public List<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();

        public static implicit operator Profile(string v)
        {
            throw new NotImplementedException();
        }
    }
}