namespace SharedServices.Extensions;

public static class StringExtensions
{
    private static readonly char[] CharactersToEscape = 
        { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

    public static string EscapeMarkdown(this string input)
    {
        return string.Concat(input.Select(c => CharactersToEscape.Contains(c) ? "\\" + c : c.ToString()));
    }
}
