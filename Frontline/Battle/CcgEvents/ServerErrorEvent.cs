namespace Frontline.Battle.CcgEvents;

public class ServerErrorEvent : CCGEventData
{
    public string ErrorMsg { get; set; }

    public ServerErrorEvent()
    {
    }

    public ServerErrorEvent(string error)
    {
        ErrorMsg = error;
    }

    public override CcgEventType Type()
    {
        return CcgEventType.ServerError;
    }
}