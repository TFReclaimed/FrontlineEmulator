namespace Frontline.Battle.CcgEvents;

public class ServerErrorEvent : CCGEventData
{
    public string ErrorMsg { get; }

    public ServerErrorEvent()
    {
    }

    public ServerErrorEvent(string error)
    {
        ErrorMsg = error;
    }

    public override CCGEventType Type()
    {
        return CCGEventType.ServerError;
    }
}