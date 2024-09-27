using System.Collections.Immutable;
using Playground.Mindustry.Blocks;
using Playground.Mindustry.Saves;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Mindustry.Maps;

public class WorldContext
{
    public Point Size { get; private set; } = new Point(10, 10);
    
    public Tile[] tiles = new Tile[100];
    public Tile this[int index]
    {
        get => tiles[index];
        set => tiles[index] = value;
    }

    public Map map;

    public SaveMeta Meta;

    public MappableContent[][] MappableContent;
    
    /** Return a tile in the tile array.*/
    public Tile tile(int index) => tiles[index];

    /** Create the tile array.*/
    public void resize(ushort width, ushort height)
    {
        Size = new Point(width, height);
        var oldTiles = tiles;
        tiles = new Tile[width * height];
        foreach (var tile in oldTiles)
        {
            if(tile != null)
                tiles[tile.y * width + tile.x] = tile;
        }
    }

    /** This should create a tile and put it into the tile array, then return it. */
    public Tile create(short x, short y, short floorID, short overlayID, short wallID)
    {
        Block? wall = null, overlay = null, floor = null;
        if (Blocks.Blocks.BlocksById.TryGetValue(wallID, out var w))
        {
            wall = new Block(w.BlockName, w.Size);
        }
        if (Blocks.Blocks.BlocksById.TryGetValue(overlayID, out w))
        {
            overlay = new Block(w.BlockName, w.Size);
        }
        if (Blocks.Blocks.BlocksById.TryGetValue(floorID, out w))
        {
            floor = new Block(w.BlockName, w.Size);
        }
        
        var tile = new Tile(wall, x, y, null, 0, floor, overlay);
        tiles[tile.y * Size.X + tile.x] = tile;
        return tile;
    }

    private PixelBlender<Rgba32>? blender;
    private ShadowState[]? renderShadows;
    public Image<Rgba32> GetMapImage()
    {
        var image = new Image<Rgba32>(Size.X, Size.Y, new Rgba32(0, 0, 0, 0));
        blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);
        renderShadows = new ShadowState[Size.X * Size.Y];
        image.ProcessPixelRows(pixels =>
        {
            for (var y = 0; y < Size.Y; y++)
            {
                var row = pixels.GetRowSpan(Size.Y - 1 - y);
                for (var x = 0; x < Size.X; x++)
                {
                    var tile = tiles[y * Size.X + x];
                    Blocks.Block b = Blocks.Blocks.blocks[tile.block.name];

                    
                    if (b.BlockName.Contains("red-diamond-wall"))
                    {
                    }
                    
                    //if (tile.team != Teams.derelict)//Block
                    if (b.hasBuilding() && b.BlockType is not "SpawnBlock") //Buildings colored by team color 
                    {
                        // b = Blocks.Blocks.blocks[tile.block.name];
                        DrawPixels(pixels, x, y, b, tile, ColorPattern.Team);
                    }
                    else if (row[x].A == 0) //Override only if team isn't rendered
                    {
                        if (tile.block?.name is not ("air" or "spawn"))
                        {
                            if (tile.overlay?.name is not ("air" or "spawn"))
                            {
                                b = Blocks.Blocks.blocks[tile.overlay.name];
                                DrawPixels(pixels, x, y, b, tile, ColorPattern.Average);
                                //row[x] = getBlockColor(tile.overlay);
                            }else
                            {
                                DrawPixels(pixels, x, y, b, tile, ColorPattern.Average);
                            }
                            //row[x] = getBlockColor(tile.block);
                        }
                        else if(tile.floor?.name is not ("air" or "spawn"))
                        {
                            if (tile.overlay?.name is not ("air" or "spawn"))
                            {
                                
                                b = Blocks.Blocks.blocks[tile.overlay.name];
                                DrawPixels(pixels, x, y, b, tile, ColorPattern.Overlay);
                                //row[x] = getBlockColor(tile.overlay);
                            }else if (tile.floor != null)
                            {
                                b = Blocks.Blocks.blocks[tile.floor.name];
                                if (b.BlockName.Contains("red-diamond-wall"))//(y == 623 && x == 766)
                                {
                        
                                }
                                DrawPixels(pixels, x, y, b, tile, ColorPattern.Shade);
                            }
                        }
                    }
                }
            }

            //Shadows
            for (int i = 0; i < Size.X * Size.Y; i++)
            {
                int x = i % Size.X;
                int y = i / Size.X;
                if (renderShadows[i] == ShadowState.Shaded)
                {
                    var tile = tiles[i];
                    if (tile.overlay == null || tile.overlay.name == "air" ||
                        tile.block == null || tile.block.name == "air")
                    {
                        var row = pixels.GetRowSpan(Size.Y -1 - y);
                        row[x] = blender.Blend(row[x], new Rgba32(0f, 0f, 0f), 0.25f);
                    }
                }
            }
        });

        blender = null;

        return image;
    }
    
    
    public Image<Rgba32> GetMapImageDebug()
    {
        var image = new Image<Rgba32>(Size.X, Size.Y, new Rgba32(0, 0, 0, 0));
        blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);
        renderShadows = new ShadowState[Size.X * Size.Y];
        image.ProcessPixelRows(pixels =>
        {
            for (var y = 0; y < Size.Y; y++)
            {
                var row = pixels.GetRowSpan(Size.Y - 1 - y);
                for (var x = 0; x < Size.X; x++)
                {
                    var tile = tiles[y * Size.X + x];
                    Blocks.Block b = Blocks.Blocks.blocks[tile.block.name];

                    //if (tile.team != Teams.derelict)//Block

                    if (tile.floor?.name is not ("air" or "spawn"))
                    {
                        b = Blocks.Blocks.blocks[tile.floor.name];
                        DrawPixels(pixels, x, y, b, tile, ColorPattern.Shade);
                    }

                }
            }
            
            image.SaveAsPng("debug1.png");
            
            for (var y = 0; y < Size.Y; y++)
            {
                var row = pixels.GetRowSpan(Size.Y - 1 - y);
                for (var x = 0; x < Size.X; x++)
                {
                    var tile = tiles[y * Size.X + x];
                    Blocks.Block b = Blocks.Blocks.blocks[tile.block.name];

                    //if (tile.team != Teams.derelict)//Block
                    
                    if (tile.block?.name is not ("air" or "spawn"))
                    {
                        DrawPixels(pixels, x, y, b, tile, ColorPattern.Average);
                        //row[x] = getBlockColor(tile.block);
                    }

                }
            }
            
            image.SaveAsPng("debug2.png");
            
            for (var y = 0; y < Size.Y; y++)
            {
                var row = pixels.GetRowSpan(Size.Y - 1 - y);
                for (var x = 0; x < Size.X; x++)
                {
                    var tile = tiles[y * Size.X + x];
                    Blocks.Block b = Blocks.Blocks.blocks[tile.block.name];

                    if (tile.overlay?.name is not ("air" or "spawn"))
                    {
                        b = Blocks.Blocks.blocks[tile.overlay.name];
                        DrawPixels(pixels, x, y, b, tile, ColorPattern.Average);
                        //row[x] = getBlockColor(tile.overlay);
                    }
                }
            }
            
            image.SaveAsPng("debug3.png");

            //Shadows
            for (int i = 0; i < Size.X * Size.Y; i++)
            {
                int x = i % Size.X;
                int y = i / Size.X;
                if (renderShadows[i] == ShadowState.Shaded)
                {
                    var tile = tiles[i];
                    if (tile.overlay == null || tile.overlay.name == "air" ||
                        tile.block == null || tile.block.name == "air")
                    {
                        var row = pixels.GetRowSpan(Size.Y -1 - y);
                        row[x] = blender.Blend(row[x], new Rgba32(0f, 0f, 0f), 0.5f);
                    }
                }
            }
        });

        blender = null;

        return image;
    }

    private enum ColorPattern
    {
        Average,
        Team,
        Shade,
        Overlay
    }
    
    private enum ShadowState
    {
        Allowed,
        Disallowed,
        Shaded
    }
    private void DrawPixels(PixelAccessor<Rgba32> pixels,int x, int y, Blocks.Block b, Tile tile, ColorPattern color = ColorPattern.Average)
    {
        // Rgba32 shade = new Rgba32(0, 0, 0, 0.5f); 
        
        var offset = b.getTileOffset();
        for (int j = 0; j < b.Size; j++)
        {
            var r = pixels.GetRowSpan(Size.Y - 1 - y - j + offset);
            for (int i = 0; i < b.Size; i++)
            {
                switch (color)
                {
                    case ColorPattern.Team:
                    {
                        
                        if(((int)tile.team) > 256)
                            continue;
                        r[x + i - offset] = teamColors[(int) tile.team];
                        renderShadows[Size.X * (y - offset + j) + (x - offset + i)] = ShadowState.Disallowed;
                        if (j == 0 && y - offset > 0)
                        {
                            if(renderShadows[Size.X * (y - offset - 1) + (x - offset + i) ] == ShadowState.Allowed)
                                renderShadows[Size.X * (y - offset - 1) + (x - offset + i) ] = ShadowState.Shaded;
                        }

                        break;
                    }
                    case ColorPattern.Shade or ColorPattern.Overlay when b.AverageColor.A > 0:
                        switch (color)
                        {
                            case ColorPattern.Shade:
                                r[x + i - offset] = blender.Blend(new Rgba32(0,0,0),
                                    b.AverageColor, 0.75f);
                                break;
                            case ColorPattern.Overlay:
                                r[x + i - offset] = b.AverageColor;
                                break;
                        }

                        break;
                    default:
                    {
                        if (b.AverageColor.A > 0)
                        {
                            if (b.BlockName.StartsWith("ore-") && Items.items.ContainsKey(b.BlockName[4..]))
                            {
                                var itm = Items.items[b.BlockName[4..]];
                                r[x + i - offset] = itm.Color;
                            }else
                            {
                                r[x + i - offset] = b.AverageColor;
                            }
                            renderShadows[Size.X * (y - offset + j) + (x - offset + i)] = ShadowState.Disallowed;
                            if (j == 0 && y - offset > 0)
                            {
                                if(renderShadows[Size.X * (y - offset - 1) + (x - offset + i) ] == ShadowState.Allowed)
                                    renderShadows[Size.X * (y - offset - 1) + (x - offset + i) ] = ShadowState.Shaded;
                            }
                        }

                        break;
                    }
                }
            }
        }
    }

    private Rgba32 getBlockColor(Block block)
    {
        var b = Blocks.Blocks.blocks[block.name];
        return b.AverageColor;
        // if (b.Regions.Count > 0)
        // {
        //     var texture = b.Regions.First().Value;
        //     return texture[texture.Size.Width / 2, texture.Size.Height / 2];
        // }
        // return new Rgba32(0, 0, 0, 0);
    }

    private static ImmutableArray<Rgba32> teamColors;

    static WorldContext()
    {
        List<Rgba32> list = new(256);

        list.Add(Rgba32.ParseHex("4d4e58"));
        list.Add(Rgba32.ParseHex("ffd37f"));
        list.Add(Rgba32.ParseHex("f25555"));
        list.Add(Rgba32.ParseHex("a27ce5"));
        list.Add(Rgba32.ParseHex("54d67d"));
        list.Add(Rgba32.ParseHex("6c87fd"));
        list.Add(Rgba32.ParseHex("e05438"));

        var rand = new Random(8);

        for (int i = 7; i < 256; i++)
        {
            var color = HSVToRgb((float)(360 * rand.NextDouble()),
                (float)(100 * ((1 - 0.4) * rand.NextDouble() + 0.4)),
                    (float)(100 * ((1 - 0.6) * rand.NextDouble() + 0.6)));
            list.Add(color);
        }
        
        teamColors = [..list];
    }

    //Source: https://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both
    private static Rgba32 HSVToRgb(double h, double s, double v)
    {

        double hh, p, q, t, ff;
        long i;
        Rgba32 output = new Rgba32()
        {
            A = 255,
        };

        if (s <= 0.0)
        {
            output.R = (byte) (v * 255);
            output.G = (byte) (v * 255);
            output.B = (byte) (v * 255);
            return output;
        }

        hh = h;
        if (hh >= 360.0) hh = 0.0;
        hh /= 60.0;
        i = (long) hh;
        ff = hh - i;
        p = v * (1.0 - s);
        q = v * (1.0 - (s * ff));
        t = v * (1.0 - (s * (1.0 - ff)));

        switch (i)
        {
            case 0:
                output.R = (byte) (v * 255);
                output.G = (byte) (t * 255);
                output.B = (byte) (p * 255);
                break;
            case 1:
                output.R = (byte) (q * 255);
                output.G = (byte) (v * 255);
                output.B = (byte) (p * 255);
                break;
            case 2:
                output.R = (byte) (p * 255);
                output.G = (byte) (v * 255);
                output.B = (byte) (t * 255);
                break;

            case 3:
                output.R = (byte) (p * 255);
                output.G = (byte) (q * 255);
                output.B = (byte) (v * 255);
                break;
            case 4:
                output.R = (byte) (t * 255);
                output.G = (byte) (p * 255);
                output.B = (byte) (v * 255);
                break;
            case 5:
            default:
                output.R = (byte) (v * 255);
                output.G = (byte) (p * 255);
                output.B = (byte) (q * 255);
                break;
        }

        return output;
    }
}