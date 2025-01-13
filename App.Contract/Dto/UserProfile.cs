namespace App.Contract.Dto
{
    public class UserProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public bool IsActive { get; set; }
        public string? fullName { get; set; }
        public List<string> roles { get; set; } = new List<string>();
    }
}
