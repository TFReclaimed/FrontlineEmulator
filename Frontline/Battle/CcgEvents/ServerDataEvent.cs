namespace Frontline.Battle.CcgEvents;

public class ServerDataEvent : CCGEventData
{
    public CCGEventType dataType;

    public int dataValue;

    public ServerDataEvent()
    {
    }

    public ServerDataEvent(CCGEventType type, int value)
    {
        dataType = type;
        dataValue = value;
    }

    public override CCGEventType Type()
    {
        return dataType;
    }
}