using System.ComponentModel.DataAnnotations;

namespace ITAssetManagement1.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères et au maximum {1} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Veuillez confirmer votre nouveau mot de passe.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; }
    }
}