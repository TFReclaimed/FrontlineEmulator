namespace Frontline.Battle.CcgEvents;

public class ServerDataEvent : CCGEventData
{
    public CCGEventType DataType { get; set; }

    public int DataValue { get; set; }

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