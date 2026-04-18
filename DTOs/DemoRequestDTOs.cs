namespace sms.api.DTOs;

public class ScheduleDemoRequestDto
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool WhatsappSame { get; set; } = true;
    public string Email { get; set; } = "";
    public string InstitutionName { get; set; } = "";
    public string InstitutionType { get; set; } = "School";
    public string BoardType { get; set; } = "CBSE";
    public string CityState { get; set; } = "";
    public string StudentStrength { get; set; } = "";
    public bool AgreeToContact { get; set; } = true;
}

public class ScheduleDemoResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}

