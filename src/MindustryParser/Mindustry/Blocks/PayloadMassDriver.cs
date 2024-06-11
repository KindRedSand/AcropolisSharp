using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class PayloadMassDriver : Block
    {
        public Image<Rgba32> baseImage, factory_in_right, factory_in_up, factory_in_left, factory_in_down,
    factory_out_right, factory_out_up, factory_out_left, factory_out_down, factory_top, driver;

        public Dictionary<Sides, Image<Rgba32>[]> RegionMap = new();


        public override void LoadRegions()
        {
            var regionSuffix = DarkRegion ? "-dark" : string.Empty;

            var path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-base");
            baseImage = LoadImage(path);

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == BlockName);
            driver = LoadImage(path);

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-in-{Size}{regionSuffix}");
            factory_in_left = LoadImage(path);
            factory_in_down = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate270));
            factory_in_right = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate180));
            factory_in_up = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate90));

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-out-{Size}{regionSuffix}");
            factory_out_right = LoadImage(path);
            factory_out_up = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            factory_out_left = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate180));
            factory_out_down = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate90));

            path = DllResource.GetAvailableResources()
                .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-top");
            if (!string.IsNullOrEmpty(path))
            {
                factory_top = LoadImage(path);
            }
            else if (Size is 3 or 5)
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-top-{Size}");
                factory_top = LoadImage(path);
            }
            {
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_left);
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_down);
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_left);
                    pixels.RenderTileOverlay(factory_in_up);
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_left);
                    pixels.RenderTileOverlay(factory_in_down);
                    pixels.RenderTileOverlay(factory_in_up);
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
            {
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_down);
                    pixels.RenderTileOverlay(factory_in_up);
                    pixels.RenderTileOverlay(factory_out_right);
                    pixels.RenderTileOverlay(factory_top);
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
                        renderPosition.Y - j + tileOffset), baseImage, new Point(i, Size - j - 1));
                    
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), RegionMap[side][tileData.rotation], new Point(i, Size - j - 1));
                    
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), driver, new Point(i, Size - j - 1));
                }
            }
        }



        public override void UpdateTiling(Schematic schem, TileData tile)
        {
            TileData? up = null, left = null, right = null, down = null;
            foreach (var t in schem.tiles)
            {
                var conn = isConnectable(tile, t, DefaultPayloads, DefaultPayloadsOmnidir);

                switch (conn)
                {
                    case Sides.Up:
                        up = t; break;
                    case Sides.Down:
                        down = t; break;
                    case Sides.Left:
                        left = t; break;
                    case Sides.Right:
                        right = t; break;
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

        public override IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            return new[] { RegionMap.First().Value[0] };
        }
    }
}
