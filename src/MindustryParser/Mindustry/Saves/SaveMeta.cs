using Playground.Mindustry.Maps;

namespace Playground.Mindustry.Saves;

public struct SaveMeta
{
    public int version;
    public int build;
    public long timestamp;
    public long timePlayed;
    public Map map;
    public string mapName;
    public string Name;
    public int wave;
    public string rules;
    public Dictionary<string, string> tags;
    public string[] mods;
    
    public SaveMeta(int version, long timestamp, long timePlayed, int build, string map, int wave, string rules, Dictionary<string, string> tags)
    {
        
    }

    public SaveMeta()
    {
        
    }
}