using JavaStream;
using Playground.Mindustry.Blocks.Build;
using Playground.Mindustry.Modules;

namespace Playground.Mindustry.Maps;

public class Tile
{
    public Block block { get; set; }
    public short x { get; set; }
    public short y { get; set; }
    public object? config { get; set; }
    public sbyte rotation { get; set; }
    public Block? floor { get; set; }
    public Block? overlay { get; set; }
    public Teams team { get; set; }

    public sbyte data;
    public Building build { get; set; }
    
    public Tile(Block block, short x, short y, object? config, sbyte rotation, Block? floor = null, Block? overlay = null, Teams team = Teams.derelict)
    {
        this.block = block;
        this.x = x;
        this.y = y;
        this.config = config;
        this.rotation = rotation;
        this.floor = floor;
        this.overlay = overlay;
        this.team = team;
    }

    public void readAll(DataInput read, sbyte revision, int len)
    {
        var b = Blocks.Blocks.blocks[block.name];
        build = b.read(this, read, revision, len);
    }

 
    
}