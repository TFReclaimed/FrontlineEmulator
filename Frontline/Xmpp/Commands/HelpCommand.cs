namespace Frontline.Xmpp.Commands;

internal sealed class HelpCommand : ChatCommand
{
    public HelpCommand() : base("help", "Shows available commands or command details.", "/help [command]", "?")
    {
    }

    public override async Task ExecuteAsync(ChatCommandContext context, string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            var commandList = string.Join(", ", context.Commands.Select(command => $"/{command.Name}"));
            await context.SendSystemMessage($"Available commands: {commandList}.");
            return;
        }

        var commandName = arguments.Trim();
        var command = context.Commands.FirstOrDefault(candidate => candidate.Matches(commandName));
        if (command is null)
        {
            await context.SendSystemMessage($"Unknown command '/{commandName}'. Type /help for a list of commands.");
            return;
        }

        await context.SendSystemMessage(command.Usage + " - " + command.Description);
    }
}