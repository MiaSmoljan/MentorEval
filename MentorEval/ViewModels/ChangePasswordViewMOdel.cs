using System.ComponentModel.DataAnnotations;

namespace MentorEval.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Trenutna lozinka je obavezna")]
        [Display(Name = "Trenutna lozinka")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova lozinka je obavezna")]
        [MinLength(6, ErrorMessage = "Lozinka mora imati najmanje 6 znakova")]
        [Display(Name = "Nova lozinka")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna")]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne podudaraju")]
        [Display(Name = "Potvrdi novu lozinku")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}