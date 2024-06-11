namespace Playground.Mindustry.Maps;

public class MappableContent
{
    public readonly String name;
    public readonly ContentType type;

    public MappableContent(String name, ContentType type)
    {
        this.name = name;
        this.type = type;
    }

    public override string ToString(){
        return name;
    }
}