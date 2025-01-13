namespace App.Contract.Dto;

public class AssignRolesDto
{
    public string userId { get; set; } = string.Empty;
    public List<string> roles{ get; set; } = new List<string>();
}
