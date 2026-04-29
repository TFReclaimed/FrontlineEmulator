namespace Frontline.Xmpp.Commands;

internal sealed class ChatCommandContext
{
    public string RoomName { get; }

    public XmppClient Sender { get; }

    public IReadOnlyList<ChatCommand> Commands { get; }

    public Func<string, Task> SendSystemMessage { get; }

    public IXmppServer XmppServer { get; }

    public ChatCommandContext(string roomName, XmppClient sender, IReadOnlyList<ChatCommand> commands,
        Func<string, Task> sendSystemMessage, IXmppServer xmppServer)
    {
        RoomName = roomName;
        Sender = sender;
        Commands = commands;
        SendSystemMessage = sendSystemMessage;
        XmppServer = xmppServer;
    }
}