namespace Ufynd.Core.Models.Responses;

public class BaseResponse<T>
{
    public int Code { get; set; }
    public string Status { get; set; }
    public T Data { get; set; }
    public bool IsSuccessful() => Code.ToString().StartsWith("20");
}