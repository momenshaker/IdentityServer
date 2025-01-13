namespace App.Contract.Dto
{
    public class PasswordResetModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
    }
}
