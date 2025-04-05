using JavaStream;

namespace Playground.Mindustry.Blocks.Build;

public class SkipBuilding : Building
{
    protected override void read(DataInput read, sbyte revision, int len)
    {
        read.Skip(len);
    }
}