namespace Playground.Mindustry.Maps;

public class Map
{
    /** Metadata. Author description, display name, etc. */
    public readonly Dictionary<string, string> tags;
    
    /** Format version. */
    public readonly int version;
    
    /** Map width/height, shorts. */
    public int width, height;

    /** Preview texture. */
    // public Texture texture;

    /** Build that this map was created in. -1 = unknown or custom build. */
    public int build;
    /** All teams present on this map.*/
    public List<int> teams = new List<int>();
    /** Number of enemy spawns on this map.*/
    public int spawns = 0;

    public Map(int width, int height, Dictionary<string, string> tags, int version, int build)
    {
        this.width = width;
        this.height = height;
        this.tags = tags;
        this.version = version;
        this.build = build;
    }
    

}

