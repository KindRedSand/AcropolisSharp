using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using JavaStream;

using System.Data;

namespace Playground.Mindustry
{
    public static class Schematics
    {
        private static readonly byte[] header = "msch"u8.ToArray();

        public static Schematic Read(string base64)
        {
            using var ms = new MemoryStream(Convert.FromBase64String(base64));

            return Read(ms);
        }

        public static Schematic Read(Stream inputStream, bool closeAfterRead = true)
        {
            foreach (var b in header)
            {
                if (inputStream.ReadByte() != b)
                {
                    throw new VersionNotFoundException("No msch header found!");
                }
            }

            var version = inputStream.ReadByte();

            if (version == 0)
            { 
                throw new VersionNotFoundException("Unsupported version!");
            }
            //PrintStream(_file);
            //return;
            using var zs = new InflaterInputStream(inputStream);
            zs.IsStreamOwner = closeAfterRead;

            var str = new DataInputStream(zs); //GetUncompressed(_file);

            //PrintStream(str);

            short width = str.ReadShort();
            short height = str.ReadShort();

            if (width > 128 || height > 128)
            {
                throw new InvalidDataException($"Scheme size are too big. Allowed 128x128 but get {width}x{height}");
            }

            var map = new Dictionary<string, string>();
            var tagCount = str.ReadUnsignedByte();

            if (tagCount > 0)
            {
                for (int i = 0; i < tagCount; i++)
                {
                    var key = str.ReadUTF();
                    var value = str.ReadUTF();
                    map[key] = value;
                }
            }

            var lenght = str.ReadByte();

            List<string> blockNames = new();
            for (int i = 0; i < lenght; i++)
            {
                var blockName = str.ReadUTF();
                blockNames.Add(blockName);
            }

            var totalBlocks = str.ReadInt();
            if (totalBlocks > 128 * 128)
            {
                throw new InvalidDataException($"Block count too big: {totalBlocks}");
            }

            List<TileData> tiles = new(totalBlocks);

            for (int i = 0; i < totalBlocks; i++)
            {
                Block block = new Block(blockNames[str.ReadUnsignedByte()]/*, Array.Empty<ItemPrice>()*/, 1);
                var posi = TypeIO.UnpackPoint(str.ReadInt());
                var config = TypeIO.ReadObject(str);

                var rotation = str.ReadByte();
                if (block.name != "air")
                {
                    tiles.Add(new TileData(block, (short)posi.X, (short)posi.Y, config, rotation));
                }
            }
            str.Close();
            return new Schematic(tiles.ToArray(), map, width, height);
        }


        public static string WriteBase64(Schematic schematic)
        {
            using var ms = new MemoryStream();
            Write(schematic, ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static void Write(Schematic schematic, Stream output)
        {
            output.Write(header);
            output.WriteByte(1);
            DataOutputStream stream = null;
            try
            {
                using var zs = new DeflaterOutputStream(output);
                stream = new DataOutputStream(zs);

                stream.WriteShort(schematic.width);
                stream.WriteShort(schematic.height);

                stream.WriteByte(schematic.tags.Count);
                foreach (var (key, val) in schematic.tags)
                {
                    stream.WriteUTF(key);
                    stream.WriteUTF(val);
                }

                List<string> blockNames = new();
                foreach (var tile in schematic.tiles)
                {
                    if(blockNames.Contains(tile.block.name))
                        continue;
                    blockNames.Add(tile.block.name);
                }
                blockNames.Sort();

                stream.WriteByte(blockNames.Count);
                foreach (string t in blockNames)
                {
                    stream.WriteUTF(t);
                }

                stream.WriteInt(schematic.tiles.Length);

                foreach (var tile in schematic.tiles)
                {
                    stream.WriteByte(blockNames.IndexOf(tile.block.name));
                    stream.WriteInt(TypeIO.PackPoint(new Point2(tile.x, tile.y)));
                    TypeIO.WriteObject(stream, tile.config);
                    stream.WriteByte(tile.rotation);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                stream?.Close();
            }
        }
    }
    
    //public record Schematic(TileData[] tiles, Dictionary<string, string> tags, int width, int height);

    public struct Schematic
    {
        public TileData[] tiles;
        public TileData[,] Tiles;
        public Dictionary<string, string> tags;
        public int width;
        public int height;

        public Schematic(TileData[] tiles, Dictionary<string, string> tags, int width, int height)
        {
            this.tiles = tiles;
            this.tags = tags;
            this.width = width;
            this.height = height;
            Tiles = new TileData[width, height];
            foreach (var tile in tiles)
            {
                Tiles[tile.x, tile.y] = tile;
            }
        }
    }
}
