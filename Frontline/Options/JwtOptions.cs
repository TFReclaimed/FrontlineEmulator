namespace Frontline.Options;

public class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Key { get; set; } = string.Empty;
}