namespace Frontline.Xmpp.Commands;

internal sealed class OnlineCountCommand : ChatCommand
{
    public OnlineCountCommand() : base("online", "Shows the amount of players online.", "/online")
    {
    }

    public override async Task ExecuteAsync(ChatCommandContext context, string arguments)
    {
        var usernames = context.XmppServer.GetOnlineUsernames();
        await context.SendSystemMessage($"Players online: {usernames.Count}.");
    }
}