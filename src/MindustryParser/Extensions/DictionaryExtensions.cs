namespace Playground.Extensions;

public static class DictionaryExtensions
{
    public static int GetInt<T>(this Dictionary<T, string> map, T key)
    {
        if (map.TryGetValue(key, out var s))
        {
            if (int.TryParse(s, out var val))
                return val;
        }
        return 0;
    }
    
    public static long GetLong<T>(this Dictionary<T, string> map, T key)
    {
        if (map.TryGetValue(key, out var s))
        {
            if (long.TryParse(s, out var val))
                return val;
        }
        return 0;
    }
    
    public static short GetShort<T>(this Dictionary<T, string> map, T key)
    {
        if (map.TryGetValue(key, out var s))
        {
            if (short.TryParse(s, out var val))
                return val;
        }
        return 0;
    }
    
    public static string Get<T>(this Dictionary<T, string> map, T key)
    {
        return map.TryGetValue(key, out string? s) ? s : string.Empty;
    }
}