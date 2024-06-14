namespace DiscordBot.Locale;

public static class LocaleHelper
{
    public static string Warnings(int count)
    {
        return count switch
        {
            0 => "нет предупреждений",
            1 => $"{count} предупреждение",
            2 or 3 or 4 => $"{count} предупреждения",
            _ => $"{count} предупреждений"
        };
    }
}