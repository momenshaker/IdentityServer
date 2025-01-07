namespace App.Contract.Dto
{
    public class UserProfile
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsMain { get; set; }
        public bool IsActive { get; set; }
        public string? fullName { get; set; }
        public List<string> roles { get; set; }
    }
}
