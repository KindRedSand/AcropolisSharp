using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class StackConveyor : Block
    {
        public Dictionary<Sides, Image<Rgba32>[]> RegionMap = new();
        Image<Rgba32> baseImage, /*baseImg_up, baseImg_left, baseImg_down,*/
            cap_right, cap_up, cap_left, cap_down;
        public override void LoadRegions()
        {
            //var list = DllResource.GetAvailableResources()
            //    .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(BlockName)).ToArray();

            if(cap_up == null)
            {
                var path = DllResource.GetAvailableResources().First(x => x.EndsWith($"{BlockName}-0.png"));
                using var str = DllResource.GetStream(path);
                baseImage = Image.Load<Rgba32>(str);
                //baseImg_up = baseImage.Clone(x => x.Rotate(RotateMode.Rotate270));
                //baseImg_left = baseImage.Clone(x => x.Rotate(RotateMode.Rotate180));
                //baseImg_down = baseImage.Clone(x => x.Rotate(RotateMode.Rotate90));
                var path2 = DllResource.GetAvailableResources().First(x => x.EndsWith($"{BlockName}-edge.png"));
                using var str2 = DllResource.GetStream(path2);
                cap_right = Image.Load<Rgba32>(str2);
                cap_up = cap_right.Clone(x => x.Rotate(RotateMode.Rotate270));
                cap_left = cap_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                cap_down = cap_right.Clone(x => x.Rotate(RotateMode.Rotate90));
            }

            {
                var img = baseImage.Clone();
                img.ProcessPixelRows(pixels =>
                {
                    //pixels.RenderTile(new Point(0,0), cap_right, Point.Empty);
                    pixels.RenderTile(new Point(0,0), cap_up, Point.Empty);
                    pixels.RenderTile(new Point(0,0), cap_left, Point.Empty);
                    pixels.RenderTile(new Point(0,0), cap_down, Point.Empty);
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
                    pixels.RenderTile(new Point(0, 0), cap_up, Point.Empty);
                    //pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
                    pixels.RenderTile(new Point(0, 0), cap_down, Point.Empty);
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
                    pixels.RenderTile(new Point(0, 0), cap_up, Point.Empty);
                    pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
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
                    pixels.RenderTile(new Point(0, 0), cap_down, Point.Empty);
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
                    pixels.RenderTile(new Point(0, 0), cap_left, Point.Empty);
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
        private bool closing = false;

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), RegionMap[side][tileData.rotation], Point.Empty);
            if (closing)
            {
                if (tileData.rotation == 0)
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), cap_right, Point.Empty);
                if (tileData.rotation == 1)
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), cap_up, Point.Empty);
                if (tileData.rotation == 2)
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), cap_left, Point.Empty);
                if (tileData.rotation == 3)
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), cap_down, Point.Empty);

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
            closing = true;

            TileData? up = null, left = null, right = null, down = null;
            foreach (var t in schem.tiles)
            {
                if (t.x == tile.x)
                {
                    if (t.y + 1 == tile.y)
                    {
                        if (isConnectable(tile, t))
                            down = t;
                        if (tile.rotation == 3 && Blocks.blocks[t.block.name].BlockType is "StackConveyor")
                        {
                            closing = false;
                        }
                    }
                    if (t.y - 1 == tile.y)
                    {
                        if (isConnectable(tile, t))
                            up = t;
                        if (tile.rotation == 1 && Blocks.blocks[t.block.name].BlockType is "StackConveyor")
                        {
                            closing = false;
                        }
                    }
                }
                else if (t.y == tile.y)
                {
                    if (t.x + 1 == tile.x)
                    {
                        if (isConnectable(tile, t))
                            left = t;
                        if (tile.rotation == 2 && Blocks.blocks[t.block.name].BlockType is "StackConveyor")
                        {
                            closing = false;
                        }
                    }
                    if (t.x - 1 == tile.x)
                    {
                        if (isConnectable(tile, t))
                            right = t;
                        if (tile.rotation == 0 && Blocks.blocks[t.block.name].BlockType is "StackConveyor")
                        {
                            closing = false;
                        }
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

            if (b.BlockType is "StackConveyor")
            {
                if (tile.rotation == 0 && tile.x + 1 == me.x)
                    return true;
                if (tile.rotation == 2 && tile.x - 1 == me.x)
                    return true;
                if (tile.rotation == 1 && tile.y + 1 == me.y)
                    return true;
                if (tile.rotation == 3 && tile.y - 1 == me.y)
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
