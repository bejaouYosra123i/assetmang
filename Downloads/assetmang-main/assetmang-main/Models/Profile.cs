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

        // New field for JobTitle (Fonction) to match the physical form's requirement
        [Required]
        public string JobTitle { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public string UserId { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

        public List<InvestmentRequest> InvestmentRequests { get; set; } = new List<InvestmentRequest>();
        

        public static implicit operator Profile(string v)
        {
            throw new NotImplementedException();
        }
    }
}