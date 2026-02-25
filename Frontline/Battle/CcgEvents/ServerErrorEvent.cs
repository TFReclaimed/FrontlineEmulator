namespace Frontline.Battle.CcgEvents;

public class ServerErrorEvent : CcgEventData
{
    public string ErrorMsg { get; set; } = string.Empty;
}