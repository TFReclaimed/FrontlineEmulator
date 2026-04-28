namespace Frontline.Xmpp.Commands;

internal abstract class ChatCommand
{
    public string Name { get; }

    public string Description { get; }

    public string Usage { get; }

    public IReadOnlyList<string> Aliases { get; }

    protected ChatCommand(string name, string description, string usage, params string[] aliases)
    {
        Name = name;
        Description = description;
        Usage = usage;
        Aliases = aliases;
    }

    public bool Matches(string command)
    {
        return string.Equals(Name, command, StringComparison.OrdinalIgnoreCase)
            || Aliases.Any(alias => string.Equals(alias, command, StringComparison.OrdinalIgnoreCase));
    }

    public abstract Task ExecuteAsync(ChatCommandContext context, string arguments);
}