namespace Frontline.Battle.CcgEvents;

public class ServerDataEvent : CCGEventData
{
    public CCGEventType DataType { get; }

    public int DataValue { get; }

    public ServerDataEvent()
    {
    }

    public ServerDataEvent(CCGEventType type, int value)
    {
        DataType = type;
        DataValue = value;
    }

    public override CCGEventType Type()
    {
        return DataType;
    }
}