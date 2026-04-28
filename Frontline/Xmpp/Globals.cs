namespace Frontline.Xmpp;

public static class Globals
{
    public const string XmppServerAddress = "prod-us-east-1-chat-lb.tfflinternal.com";
    public const string XmppMucAddress = $"conference.{XmppServerAddress}";
    public const int MaxMessages = 40;
    public const int MaxMessageLength = 140;
}