using System.ComponentModel.DataAnnotations;

namespace App.Contract.Dto
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        [Display(Name = "CountryCode")]
        public string CountryCode { get; set; } = string.Empty;
        [Display(Name = "Roles")]
        public List<string>? Roles { get; set; }
        [Required]
        [Display(Name = "FullName")]
        public string FullName { get; set; } = string.Empty;

    }
}
