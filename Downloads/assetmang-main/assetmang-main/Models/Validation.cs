namespace ITAssetManagement1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class Validation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } // e.g., "Manager", "IT Manager", "HR Manager", "Plant Manager"

        [StringLength(200)]
        public string Remark { get; set; }

        [StringLength(100)]
        public string Signature { get; set; }

        public int ITRequestId { get; set; } // Foreign key to the ITRequest
        public ITRequest ITRequest { get; set; } // Navigation property
    }

    public class ITRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequesterName { get; set; }

        [Required]
        [StringLength(50)]
        public string Department { get; set; }

        [Required]
        [StringLength(50)]
        public string Fonction { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public string RequesterId { get; set; } // Foreign key to IdentityUser
        public IdentityUser Requester { get; set; } // Navigation property

        // Collection of validations associated with this request
        public List<Validation> Validations { get; set; } = new List<Validation>();
    }

    public class PcRequest : ITRequest
    {
        [Required]
        [StringLength(50)]
        public string PcType { get; set; } // Now a string field for manual input (e.g., "Desktop", "Laptop")

        [Required]
        [StringLength(500)]
        public string NeedDescription { get; set; }
    }
}