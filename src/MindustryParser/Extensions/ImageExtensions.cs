using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Extensions
{
    public static class ImageExtensions
    {
        public const int TileSize = 32;

        public static void RenderTile(this PixelAccessor<Rgba32> pixels, Point tilePosition, Image<Rgba32> image, Point imageTile, byte alphaOverride = 0)
        {
            int px = imageTile.X * TileSize;
            int py = imageTile.Y * TileSize;
            var blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);
            for (int j = tilePosition.Y * TileSize; j < (tilePosition.Y * TileSize) + TileSize; j++, py++)
            {
                var row = pixels.GetRowSpan(j);
                for (int i = tilePosition.X * TileSize; i < (tilePosition.X * TileSize) + TileSize; i++, px++)
                {
                    if(image[px, py].A == 0)
                        continue;
                    if (alphaOverride == 0)
                    {
                        if (image[px, py].A == 255)
                            row[i] = image[px, py];
                        else
                        {
                            row[i] = blender.Blend(row[i], image[px, py],
                                image[px, py].A/255f); 
                        }
                    }
                    else
                    {
                        var c = image[px, py];
                        var or = row[i];
                        var alpha = alphaOverride / 255f;
                        var cc = blender.Blend(or, c, alpha);
                        row[i] = cc;
                    }
                }
                px = imageTile.X * TileSize;
            }
        }

        public static void RenderTileOverlay(this PixelAccessor<Rgba32> pixels, Image<Rgba32> image, byte alphaOverride = 0)
        {
            int px = 0;
            int py = 0;
            var blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);
            for (int j = 0; j < pixels.Height && j < image.Height; j++, py++)
            {
                var row = pixels.GetRowSpan(j);
                for (int i = 0; i < pixels.Width && i < image.Width; i++, px++)
                {
                    if (image[px, py].A == 0)
                        continue;

                    if (alphaOverride == 0)
                    {
                        if (image[px, py].A == 255)
                            row[i] = image[px, py];
                        else
                        {
                            row[i] = blender.Blend(row[i], image[px, py],
                                image[px, py].A / 255f); //Blend(row[i], image[px, py]);
                        }
                    }
                    else
                    {
                        var c = image[px, py];
                        var or = row[i];
                        var alpha = alphaOverride / 255f;
                        var cc = blender.Blend(or, c, alpha);
                        row[i] = cc;
                    }
                }
                px = 0;
            }
        }

        public static void RenderTileOverlayWithShadow(this PixelAccessor<Rgba32> pixels, Image<Rgba32> image, Rgba32 shadowColor, byte shadowSize = 2)
        {
            int px = 0;
            int py = 0;
            var blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);

            for (int shadowY = -shadowSize; shadowY <= shadowSize; shadowY++)
            {
                for (int shadowX = -shadowSize; shadowX <= shadowSize; shadowX++)
                {
                    px = shadowX;
                    py = shadowY;

                    for (int j = 0; j < pixels.Height && py < pixels.Height; j++, py++)
                    {
                        if (py < 0)
                        {
                            continue;
                        }
                        var row = pixels.GetRowSpan(j);
                        for (int i = 0; i < pixels.Width && px < pixels.Width; i++, px++)
                        {
                            if (px < 0 || image[px, py].A == 0)
                            {
                                continue;
                            }
                            
                            row[i] = shadowColor;
                        }

                        px = shadowX;
                    }
                }
            }

            px = py = 0;
            for (int j = 0; j < pixels.Height && j < image.Height; j++, py++)
            {
                var row = pixels.GetRowSpan(j);
                for (int i = 0; i < pixels.Width && i < image.Width; i++, px++)
                {
                    if (image[px, py].A == 0)
                        continue;

                    row[i] = blender.Blend(row[i], image[px, py],
                        image[px, py].A / 255f);
                }
                px = 0;
            }
        }


        public static void RenderTileMask(this PixelAccessor<Rgba32> pixels, Point tilePosition, Image<Rgba32> maskImage, Point imageTile, Color maskColor, byte alphaOverride = 0)
        {
            int px = imageTile.X * TileSize;
            int py = imageTile.Y * TileSize;

            for (int j = tilePosition.Y * TileSize; j < (tilePosition.Y * TileSize) + TileSize; j++, py++)
            {
                var row = pixels.GetRowSpan(j);
                for (int i = tilePosition.X * TileSize; i < (tilePosition.X * TileSize) + TileSize; i++, px++)
                {
                    if (maskImage[px, py].A == 0)
                        continue;

                    if (alphaOverride == 0)
                    {
                        if (maskImage[px, py].A == 255)
                            row[i] = maskColor;
                        else
                        {
                            row[i] = Blend(row[i], maskColor);
                        }
                    }
                    else
                    {
                        var c = maskColor.WithAlpha(alphaOverride);
                        row[i] = Blend(row[i], c);
                    }
                }

                px = imageTile.X * TileSize;
            }
        }


        public static void RenderTileShifted(this PixelAccessor<Rgba32> pixels, float x, float y, Image<Rgba32> image, Point imageTile, byte alphaOverride = 0)
        {
            int px = imageTile.X * TileSize;
            int py = imageTile.Y * TileSize;

            for (int j = (int)Math.Floor(y * TileSize); j < (y * TileSize) + TileSize && py < imageTile.Y * TileSize + image.Height; j++, py++)
            {
                var row = pixels.GetRowSpan(j);
                for (int i = (int)Math.Floor(x * TileSize); i < (x * TileSize) + TileSize && px < imageTile.X * TileSize + image.Height; i++, px++)
                {
                    if (image[px, py].A == 0)
                        continue;

                    if (alphaOverride == 0)
                    {
                        if (image[px, py].A == 255)
                            row[i] = image[px, py];
                        else
                        {
                            row[i] = Blend(row[i], image[px, py]);
                        }
                    }
                    else
                    {
                        var c = image[px, py];
                        c.A = alphaOverride;
                        row[i] = Blend(row[i], c);
                    }
                }

                px = imageTile.X * TileSize;
            }
        }


        public static void Modulate(this Image<Rgba32> image, Color color)
        {
            var col = color.ToPixel<Rgba32>();
            image.ProcessPixelRows(pixels =>
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var row = pixels.GetRowSpan(j);
                    for (int i = 0; i < image.Width; i++)
                    {
                        if (row[i].A == 0)
                            continue;
                        var c = row[i];
                      
                        c.R = (byte)((((float)c.R)/255) * col.R);
                        c.G = (byte)((((float)c.G)/255) * col.G);
                        c.B = (byte)((((float)c.B)/255) * col.B);
                        if(col.A != 255)
                            c.A = (byte) ((((float) c.A) / 255) * col.A);
                        row[i] = c;
                    }
                }
            });
        }


        public static Rgba32 Blend(Rgba32 from, Rgba32 to, bool noAlphaBlending = false)
        {
            var r = (to.R * to.A / 255) + (from.R * from.A * (255 - to.A) / (255 * 255));
            var g = (to.G * to.A / 255) + (from.G * from.A * (255 - to.A) / (255 * 255));
            var b = (to.B * to.A / 255) + (from.B * from.A * (255 - to.A) / (255 * 255));
            var a= noAlphaBlending ? from.A : to.A + (from.A * (255 - to.A) / 255);
            return new Rgba32((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
