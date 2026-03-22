namespace Frontline.Battle.CcgEvents;

public class ServerDataEvent : CcgEventData
{
    public CcgEventType DataType { get; set; }

    public int DataValue { get; set; }
}