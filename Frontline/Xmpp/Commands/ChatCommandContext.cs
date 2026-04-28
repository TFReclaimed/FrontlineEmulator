namespace Frontline.Xmpp.Commands;

internal sealed class ChatCommandContext
{
    public string RoomName { get; }

    public XmppClient Sender { get; }

    public IReadOnlyList<ChatCommand> Commands { get; }

    public Func<string, Task> SendSystemMessage { get; }

    public ChatCommandContext(string roomName, XmppClient sender, IReadOnlyList<ChatCommand> commands,
        Func<string, Task> sendSystemMessage)
    {
        RoomName = roomName;
        Sender = sender;
        Commands = commands;
        SendSystemMessage = sendSystemMessage;
    }
}