namespace Frontline.Xmpp.Commands;

internal sealed class AlertCommand : ChatCommand
{
    public AlertCommand() : base("alert", "Sends an alert popup to everyone.", "/alert <message>", true)
    {
    }

    public override async Task ExecuteAsync(ChatCommandContext context, string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            await context.SendSystemMessage($"Usage: {Usage}");
            return;
        }

        var message = arguments.Trim();
        await context.XmppServer.BroadcastSystemMessageAsync(":::ALERT", message);
    }
}