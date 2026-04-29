namespace Frontline.Xmpp.Commands;

internal sealed class MuteCommand : ChatCommand
{
    private const int DefaultMuteMinutes = 10;

    public MuteCommand() : base("mute", "Mutes a user by id.", "/mute <userId> [minutes]")
    {
    }

    public override async Task ExecuteAsync(ChatCommandContext context, string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            await context.SendSystemMessage($"Usage: {Usage}");
            return;
        }

        var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!int.TryParse(parts[0], out var userId) || userId <= 0)
        {
            await context.SendSystemMessage($"Invalid user id. Usage: {Usage}.");
            return;
        }

        var muteMinutes = DefaultMuteMinutes;
        if (parts.Length > 1)
        {
            if (!int.TryParse(parts[1], out muteMinutes) || muteMinutes <= 0)
            {
                await context.SendSystemMessage("Invalid mute duration. Minutes must be a positive number.");
                return;
            }
        }

        var muted = await context.XmppServer.MuteUserAsync(userId, TimeSpan.FromMinutes(muteMinutes));
        if (!muted)
        {
            await context.SendSystemMessage($"Could not mute user {userId}. Player was not found.");
            return;
        }

        await context.SendSystemMessage($"User {userId} has been muted for {muteMinutes} minute(s).");
    }
}