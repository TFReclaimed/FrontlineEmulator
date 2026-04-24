namespace Frontline.Options;

[OptionsSection("GameSettings")]
public class GameOptions
{
    public string MinClientVersion { get; set; } = "1.0.15816";
    public bool EnableMatchmaking { get; set; } = true;
    public bool EnableChat { get; set; } = true;
}