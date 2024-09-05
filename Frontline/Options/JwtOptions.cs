namespace Frontline.Options;

[OptionsSection("JwtSettings")]
public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
}