namespace TelegramAPI.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal sealed class CommandAttribute : Attribute
{
    public string CommandName { get; }
    public string Description { get; }

    public CommandAttribute(string commandName, string description)
    {
        CommandName = commandName;
        Description = description;
    }
}
