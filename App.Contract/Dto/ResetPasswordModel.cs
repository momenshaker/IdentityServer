using System.ComponentModel.DataAnnotations;

namespace App.Contract.Dto
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
