namespace Frontline.Battle.CcgEvents;

public class ServerDataEvent : CCGEventData
{
    public CcgEventType DataType { get; set; }

    public int DataValue { get; set; }

    public ServerDataEvent()
    {
    }

    public ServerDataEvent(CcgEventType type, int value)
    {
        DataType = type;
        DataValue = value;
    }

    public override CcgEventType Type()
    {
        return DataType;
    }
}