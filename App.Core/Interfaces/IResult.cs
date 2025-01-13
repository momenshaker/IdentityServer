namespace App.Core.Interfaces;

public interface IResult<T>
{
    string Version { get; set; }
    List<string>? ErrorMessages { get; set; }
    DateTime TimeStamp { get; set; }
}
