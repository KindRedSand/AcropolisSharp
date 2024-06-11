using JavaStream;
using Playground.Extensions;
using Playground.Mindustry.Maps;

namespace Playground.Mindustry.Saves;

public class SaveVersion : SaveFileReader
{
    //Mods related
    //protected static Dictionary<String, CustomChunk> customChunks = new ();
    public readonly int version = 8;
    
    public SaveMeta getMeta(DataInput stream)
    {
        stream.ReadInt(); //length of data, doesn't matter here
        Dictionary<string, string> map = readStringMap(stream);
        return new SaveMeta
        {
            version = map.GetInt("version"),
            timestamp = map.GetLong("saved"),
            timePlayed = map.GetLong("playtime"),
            build = map.GetInt("build"),
            mapName = map.Get("mapname"),
            wave = map.GetInt("wave"),
            tags = map,
            rules = map.Get("rules")
        };
    }

    // public Stream Original;
    public override void read(DataInput stream, WorldContext context)
    {
        region("meta", stream,  x => readMeta(x, context));
        region("content", stream, x => readContentHeader(x, context));

        try{
            region("map", stream,  x => readMap(x, context));
            //region("entities", stream, x => readEntities(x, context));
            //Can't be read bcz i don't want read units
            // if(version >= 8) region("markers", stream, x => readMarkers(x, context));
            //Mods are not supported
            //region("custom", stream, readCustomChunks);
        }finally{
            // content.setTemporaryMapper(null);
        }
    }

    public void readMap(DataInput stream, WorldContext context)
    {
        var width = (ushort)stream.ReadUnsignedShort();
        var height = (ushort)stream.ReadUnsignedShort();

        try
        {
            context.resize(width, height);
            
            //read floor and create tiles first
            for(int i = 0; i < width * height; i++){
                int x = i % width, y = i / width;
                short floorid = stream.ReadShort();
                short oreid = stream.ReadShort();
                int consecutives = stream.ReadUnsignedByte();

                // context.MappableContent[(int) ContentType.Block][floorid];
                //if(content.block(floorid) == Blocks.air) floorid = Blocks.stone.id;

                context.create((short)x, (short)y, floorid, oreid, 0);

                for(int j = i + 1; j < i + 1 + consecutives; j++){
                    int newx = j % width, newy = j / width;
                    context.create((short)newx, (short)newy, floorid, oreid, (short)0);
                }

                i += consecutives;
            }

            //read blocks
            for (int i = 0; i < width * height; i++)
            {
                // Console.WriteLine($"{i}");
                short blockId = stream.ReadShort();
                Blocks.Blocks.BlocksById.TryGetValue(blockId, out var b);
                var block = b != null ? new Block(b.BlockName, b.Size) : new Block("air", 1);
                var tile = context.tile(i);
                bool isCenter = true;
                sbyte packedCheck = stream.ReadByte();
                bool hadEntity = (packedCheck & 1) != 0;
                bool hadData = (packedCheck & 2) != 0;

                if (hadEntity)
                {
                    isCenter = stream.ReadBoolean();
                }

                //set block only if this is the center; otherwise, it's handled elsewhere
                if (isCenter)
                {
                    tile.block = block;
                }

                if (hadEntity)
                {
                    if (isCenter)//only read entity for center blocks
                    {
                        if (b?.hasBuilding() ?? false)
                        {
                            try
                            {
                                readSkippableChunk(stream, true, (x, len) => {
                                    sbyte revision = x.ReadByte();
                                    tile.readAll(x, revision, len-1);
                                    tile.team = tile.build?.team ?? Teams.sharded;
                                });
                                // skipChunk(stream, true);
                            }
                            catch (Exception e)
                            {
                                throw new IOException("Failed to read tile entity of block: " + block, e);
                            }
                        }
                        else
                        {
                            //skip the entity region, as the entity and its IO code are now gone
                            skipChunk(stream, true);
                        }

                    }
                }
                else if (hadData)
                {
                    tile.block = block;
                    tile.data = stream.ReadByte();
                }
                else
                {
                    int consecutives = stream.ReadUnsignedByte();

                    for (int j = i + 1; j < i + 1 + consecutives && j < width * height; j++)
                    {
                        context[j].block = block;
                    }

                    i += consecutives;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void readContentHeader(DataInput stream, WorldContext context)
    {
        sbyte mapped = stream.ReadByte();
        var map = context.MappableContent = new MappableContent[Enum.GetValuesAsUnderlyingType<ContentType>().Length][];
        
        for(int i = 0; i < mapped; i++){
            ContentType type = (ContentType)stream.ReadByte();
            short total = stream.ReadShort();
            map[(int)type] = new MappableContent[total];

            for(int j = 0; j < total; j++){
                String name = stream.ReadUTF();
                //fallback only for blocks
                if (type == ContentType.Block)
                {
                    if (fallback.TryGetValue(name, out string? value))
                        name = value;
                }
                map[(int) type][j] = new MappableContent(name, type);
            }
        }
    }
    
    public void readMeta(DataInput stream, WorldContext context)
    {
        Dictionary<string, string> map = readStringMap(stream);
        context.Meta = new SaveMeta
        {
            version = map.GetInt("version"),
            timestamp = map.GetLong("saved"),
            timePlayed = map.GetLong("playtime"),
            build = map.GetInt("build"),
            mapName = map.Get("name"),
            wave = map.GetInt("wave"),
            tags = map,
            rules = map.Get("rules")
        };
    }
    
    
    public void readEntities(DataInput stream, WorldContext context)
    {
        readEntityMapping(stream);
        readTeamBlocks(stream);
        // readWorldEntities(stream);//Pain in the ass, but seems like we can safely ignore it
    }
    
    public void readEntityMapping(DataInput stream)
    {
        short amount = stream.ReadShort();
        for (int i = 0; i < amount; i++)
        {
            short id = stream.ReadShort();
            String name = stream.ReadUTF();
            //entityMapping[id] = EntityMapping.map(name);
        }
    }

    public void readTeamBlocks(DataInput stream)
    {
        int teamc = stream.ReadInt();

        for (int i = 0; i < teamc; i++)
        {
            Team team = (Team)(stream.ReadInt());
            int blocks = stream.ReadInt();
            // data.plans.clear();
            // data.plans.ensureCapacity(Math.min(blocks, 1000));
            var set = new List<Point2>();

            for (int j = 0; j < blocks; j++)
            {
                short x = stream.ReadShort(),
                    y = stream.ReadShort(),
                    rot = stream.ReadShort(),
                    bid = stream.ReadShort();
                var obj = TypeIO.ReadObject(stream);
                //cannot have two in the same position
                // if (set.add(Point2.pack(x, y)))
                // {
                //     data.plans.addLast(new BlockPlan(x, y, rot, content.block(bid).id, obj));
                // }
            }
        }
    }
}