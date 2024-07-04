using Playground.Extensions;
using Playground.Resource;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks;

public class Duct : Block
{
    public Dictionary<Sides, Image<Rgba32>[]> RegionMap = new();

    public override void LoadRegions()
    {
        var list = DllResource.GetAvailableResources()
            .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(BlockName)).ToArray();

        Image<Rgba32> img0, img1, img2, img4;
        Color bottom = Color.Parse("4A4B53");
        //4A4B53
        {
            var path = DllResource.GetAvailableResources()
                .First(x => x.EndsWith("duct-bottom-0.png"));
            using var str = DllResource.GetStream(path);
            img0 = Image.Load<Rgba32>(str);
            img0.ProcessPixelRows(pixels =>
            {
                for (int j = 0; j < pixels.Height; j++)
                {
                    var row = pixels.GetRowSpan(j);
                    for (int i = 0; i < pixels.Width; i++)
                    {
                        if (row[i].A > 0)
                        {
                            row[i] = bottom;
                        }
                    }
                }
            });
        }
        {
            var path = DllResource.GetAvailableResources()
                .First(x => x.EndsWith("duct-bottom-1.png"));
            using var str = DllResource.GetStream(path);
            img1 = Image.Load<Rgba32>(str);
            img1.ProcessPixelRows(pixels =>
            {
                for (int j = 0; j < pixels.Height; j++)
                {
                    var row = pixels.GetRowSpan(j);
                    for (int i = 0; i < pixels.Width; i++)
                    {
                        if (row[i].A > 0)
                        {
                            row[i] = bottom;
                        }
                    }
                }
            });
        }
        {
            var path = DllResource.GetAvailableResources()
                .First(x => x.EndsWith("duct-bottom-2.png"));
            using var str = DllResource.GetStream(path);
            img2 = Image.Load<Rgba32>(str);
            img2.ProcessPixelRows(pixels =>
            {
                for (int j = 0; j < pixels.Height; j++)
                {
                    var row = pixels.GetRowSpan(j);
                    for (int i = 0; i < pixels.Width; i++)
                    {
                        if (row[i].A > 0)
                        {
                            row[i] = bottom;
                        }
                    }
                }
            });
        }

        foreach (string path in list)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            var arr = filename[(BlockName.Length + 1)..].Split('-');
            if (arr[0] is "bottom" or "cap" or "bridge" or "router" or "unloader")
                continue;

            if(!int.TryParse(arr[1], out var v1))
                continue;

            if (v1 == 0)
            {
                using var str = DllResource.GetStream(path);
                var img = Image.Load<Rgba32>(str);
                img.ProcessPixelRows(pixels =>
                {
                    for (int j = 0; j < pixels.Height; j++)
                    {
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width; i++)
                        {
                            if (row[i].A == 0 && img0[i, j].A > 0)
                            {
                                row[i] = img0[i, j];
                            }
                        }
                    }
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.None, imgs);
                RegionMap.Add(Sides.Back, imgs);
            }
            else if (v1 == 1)
            {
                using var str = DllResource.GetStream(path);
                var img = Image.Load<Rgba32>(str);
                img.ProcessPixelRows(pixels =>
                {
                    for (int j = 0; j < pixels.Height; j++)
                    {
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width; i++)
                        {
                            if (row[i].A == 0 && img1[i, j].A > 0)
                            {
                                row[i] = img1[i, j];
                            }
                        }
                    }
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left, imgs);

                img = img.Clone(x => x.Flip(FlipMode.Vertical));
                imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Right, imgs);
            }
            else if (v1 == 2)
            {
                using var str = DllResource.GetStream(path);
                var img = Image.Load<Rgba32>(str);
                img.ProcessPixelRows(pixels =>
                {
                    for (int j = 0; j < pixels.Height; j++)
                    {
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width; i++)
                        {
                            if (row[i].A == 0 && img2[i, j].A > 0)
                            {
                                row[i] = img2[i, j];
                            }
                        }
                    }
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Right | Sides.Back, imgs);
                
                img = img.Clone(x => x.Flip(FlipMode.Vertical));
                imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                
                RegionMap.Add(Sides.Left | Sides.Back, imgs);
            }
            else if (v1 == 3)
            {
                using var str = DllResource.GetStream(path);
                var img = Image.Load<Rgba32>(str);
                img.ProcessPixelRows(pixels =>
                {
                    for (int j = 0; j < pixels.Height; j++)
                    {
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width; i++)
                        {
                            if (row[i].A == 0 && img2[i, j].A > 0)
                            {
                                row[i] = img2[i, j];
                            }
                        }
                    }
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left | Sides.Right | Sides.Back, imgs);
            }
            else if (v1 == 4)
            {
                using var str = DllResource.GetStream(path);
                var img = Image.Load<Rgba32>(str);
                img.ProcessPixelRows(pixels =>
                {
                    for (int j = 0; j < pixels.Height; j++)
                    {
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width; i++)
                        {
                            if (row[i].A == 0 && img2[i, j].A > 0)
                            {
                                row[i] = img2[i, j];
                            }
                        }
                    }
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left | Sides.Right, imgs);
            }

        }

    }

    private Sides side = Sides.None;

    public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
    {
        pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), RegionMap[side][tileData.rotation],
            Point.Empty);
    }


    // 0 = Right
    // 1 = Up
    // 2 = Left
    // 3 = Down
    //    Y+
    // X- ## x+
    //    Y-
    public override void UpdateTiling(Schematic schem, TileData tile)
    {
        //UpdateTilingOld(schem, tile);
        //return;

        TileData? up = null, left = null, right = null, down = null;
        foreach (var t in schem.tiles)
        {
            var b = Blocks.blocks[t.block.name];
            var offset = b.getTileOffset();
            var rect = new Rectangle(t.x - offset, t.y - offset, b.Size, b.Size);
                
            if (rect.Contains(tile.x, tile.y - 1))
            {
                if (isConnectable(tile, t))
                    down = t;
            }
            if (rect.Contains(tile.x, tile.y + 1))
            {
                if (isConnectable(tile, t))
                    up = t;
            }
            if (rect.Contains(tile.x - 1, tile.y))
            {
                if (isConnectable(tile, t))
                    left = t;
            }
            if (rect.Contains(tile.x + 1, tile.y))
            {
                if (isConnectable(tile, t))
                    right = t;
            }
        }

        side = Sides.None;

        switch (tile.rotation)
        {
            case 0:
            {
                if (up != null)
                    side |= Sides.Left;
                if (left != null)
                    side |= Sides.Back;
                if (down != null)
                    side |= Sides.Right;
                break;
            }
            case 2:
            {
                if (up != null)
                    side |= Sides.Right;
                if (right != null)
                    side |= Sides.Back;
                if (down != null)
                    side |= Sides.Left;
                break;
            }
            case 1:
            {
                if (left != null)
                    side |= Sides.Left;
                if (down != null)
                    side |= Sides.Back;
                if (right != null)
                    side |= Sides.Right;
                break;
            }
            case 3:
            {
                if (right != null)
                    side |= Sides.Left;
                if (up != null)
                    side |= Sides.Back;
                if (left != null)
                    side |= Sides.Right;
                break;
            }
        }
    }

    private bool isConnectable(TileData me, TileData tile)
    {
        var b = Blocks.blocks[tile.block.name];

        if (BlockType != "ArmoredDuct" && DefaultDistributionOmnidir.Invoke(tile))
            return true;

        if (b.BlockType is "Conveyor" or "ArmoredConveyor" or "Duct" or "ArmoredDuct" or "DuctBridge")
        {
            switch (tile.rotation)
            {
                case 0 when tile.x + 1 == me.x:
                case 2 when tile.x - 1 == me.x:
                case 1 when tile.y + 1 == me.y:
                case 3 when tile.y - 1 == me.y:
                    return true;
                default:
                    return false;
            }
        }

        return false;
    }

    public override IEnumerable<Image<Rgba32>> GetRenderSprites()
    {
        return new[] {RegionMap.First().Value[0]};
    }
}