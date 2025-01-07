namespace App.Contract.Dto
{
    public class PasswordResetModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Path { get; set; }
        public string Event { get; set; }
    }
}
