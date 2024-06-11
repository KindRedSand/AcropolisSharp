using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class PayloadConveyor : Block
    {
        public Dictionary<Sides, Image<Rgba32>[]> RegionMap = new();
        Image<Rgba32> baseImage, baseImage_right, baseImage_up, baseImage_left, baseImage_down,
            cap_right, cap_up, cap_left, cap_down;

        public override void LoadRegions()
        {
            //Payload conveyors don't have size assigns in Blocks.java and always 3x3
            Size = 3;


            var apath = DllResource.GetAvailableResources().First(x => x.EndsWith($"{BlockName}-top.png"));
            using var astr = DllResource.GetStream(apath);
            var arrow = Image.Load<Rgba32>(astr);

            var path = DllResource.GetAvailableResources().First(x => x.EndsWith($"{BlockName}.png"));
            using var str = DllResource.GetStream(path);
            baseImage_right = baseImage = Image.Load<Rgba32>(str);
            baseImage_right.ProcessPixelRows(pixels =>
            {
                for (int j = 0; j < Size; j++)
                {
                    for (int i = 0; i < Size; i++)
                    {
                        pixels.RenderTile(new Point(i, j), arrow, new Point(i, j));
                    }
                }
            });

            baseImage_up = baseImage_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            baseImage_left = baseImage_right.Clone(x => x.Rotate(RotateMode.Rotate180));
            baseImage_down = baseImage_right.Clone(x => x.Rotate(RotateMode.Rotate90));

            var path2 = DllResource.GetAvailableResources().First(x => x.EndsWith($"{BlockName}-edge.png"));
            using var str2 = DllResource.GetStream(path2);
            cap_right = Image.Load<Rgba32>(str2);
            cap_up = cap_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            cap_left = cap_right.Clone(x => x.Rotate(RotateMode.Rotate180));
            cap_down = cap_right.Clone(x => x.Rotate(RotateMode.Rotate90));


            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    pixels.RenderTileOverlay(cap_up);
                    //pixels.RenderTileOverlay(cap_left);
                    pixels.RenderTileOverlay(cap_down);
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.None, imgs);
            }

            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    pixels.RenderTileOverlay(cap_up);
                    //pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
                    pixels.RenderTileOverlay(cap_down);
                });
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Back, imgs);
            }
            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    pixels.RenderTileOverlay(cap_up);
                    pixels.RenderTileOverlay(cap_left);
                    //pixels.RenderTile(new Point(0, 0), cap_down, Point.Empty);
                });

                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Right, imgs);

                img = img.Clone(x => x.Flip(FlipMode.Vertical));

                imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left, imgs);
            }
            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    //pixels.RenderTile(new Point(0, 0), cap_up, Point.Empty);
                    //pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
                    pixels.RenderTileOverlay(cap_down);
                });

                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left | Sides.Back, imgs);

                img = img.Clone(x => x.Flip(FlipMode.Vertical));
                imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Right | Sides.Back, imgs);
            }
            {
                var img = baseImage.Clone();
                //img.ProcessPixelRows(pixels =>
                //{
                //    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                //    pixels.RenderTile(new Point(0, 0), cap_up, Point.Empty);
                //    pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
                //    pixels.RenderTile(new Point(0, 0), cap_down, Point.Empty);
                //});
                var imgs = new Image<Rgba32>[]
                {
                    img,
                    img.Clone(x => x.Rotate(RotateMode.Rotate270)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate180)),
                    img.Clone(x => x.Rotate(RotateMode.Rotate90)),
                };
                RegionMap.Add(Sides.Left | Sides.Right | Sides.Back, imgs);
            }
            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    //pixels.RenderTile(new Point(0, 0), cap_up, Point.Empty);
                    pixels.RenderTileOverlay(cap_left);
                    //pixels.RenderTile(new Point(0, 0), cap_down, Point.Empty);
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

        private Sides side = Sides.None;

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var tileOffset = getTileOffset();
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), RegionMap[side][tileData.rotation], new Point(i, Size - j - 1));
                }
            }
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
            TileData? up = null, left = null, right = null, down = null;
            foreach (var t in schem.tiles)
            {
                if (t.x == tile.x)
                {
                    if (t.y + 3 == tile.y)
                    {
                        if (isConnectable(tile, t))
                            down = t;
                    }
                    if (t.y - 3 == tile.y)
                    {
                        if (isConnectable(tile, t))
                            up = t;
                    }
                }
                else if (t.y == tile.y)
                {
                    if (t.x + 3 == tile.x)
                    {
                        if (isConnectable(tile, t))
                            left = t;
                    }
                    if (t.x - 3 == tile.x)
                    {
                        if (isConnectable(tile, t))
                            right = t;
                    }
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

            if (b.BlockType is "PayloadConveyor" or "PayloadRouter" or "PayloadMassDriver" or "Reconstructor"
                or "UnitFactory" or "Constructor" or "PayloadUnloader" or "PayloadLoader")
            {
                if (tile.rotation == 0 && tile.x + 3 == me.x)
                    return true;
                if (tile.rotation == 2 && tile.x - 3 == me.x)
                    return true;
                if (tile.rotation == 1 && tile.y + 3 == me.y)
                    return true;
                if (tile.rotation == 3 && tile.y - 3 == me.y)
                    return true;
                return false;
            }

            return false;
        }

        public override IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            return new[] { RegionMap.First().Value[0] };
        }

        [Flags]
        public enum Sides
        {
            None = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Back = 1 << 2,
        }
    }
}
