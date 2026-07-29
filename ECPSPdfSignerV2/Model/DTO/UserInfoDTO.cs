namespace ECPSPdfSignerV2.Model.DTO; 

public class UserInfoDTO
{
    public string name { get; set; } = string.Empty;

    public List<string> roles { get; set; } = new();
}
