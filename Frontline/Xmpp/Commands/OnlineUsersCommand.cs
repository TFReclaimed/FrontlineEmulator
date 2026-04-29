namespace Frontline.Xmpp.Commands;

internal sealed class OnlineUsersCommand : ChatCommand
{
    public OnlineUsersCommand() : base("who", "Shows a list of online players.", "/who")
    {
    }

    public override async Task ExecuteAsync(ChatCommandContext context, string arguments)
    {
        var usernames = context.XmppServer.GetOnlineUsernames();
        if (usernames.Count == 0)
        {
            await context.SendSystemMessage("No players are currently online.");
            return;
        }

        await context.SendSystemMessage($"Online: {string.Join(", ", usernames)}");
    }
}