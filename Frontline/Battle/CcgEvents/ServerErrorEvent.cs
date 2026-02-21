namespace Frontline.Battle.CcgEvents;

public class ServerErrorEvent : CCGEventData
{
    public string errorMsg;

    public ServerErrorEvent()
    {
    }

    public ServerErrorEvent(string error)
    {
        errorMsg = error;
    }

    public override CCGEventType Type()
    {
        return CCGEventType.ServerError;
    }
}