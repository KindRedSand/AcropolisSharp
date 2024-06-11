using System.Data;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using JavaStream;
using Playground.Mindustry.Maps;
using Playground.Mindustry.Saves;

namespace Playground.Mindustry;

public static class SaveIO
{
    private static readonly byte[] header = "MSAV"u8.ToArray();

    private static SaveVersion save = new();

    public static SaveMeta GetMeta(DataInputStream stream)
    {
        try
        {
            foreach (var b in header)
            {
                if (stream.ReadByte() != b)
                {
                    throw new VersionNotFoundException("No MSAV header found!");
                }
            }

            int version = stream.ReadInt();
            //I don't want to bother with legacyreader for now
            return save.getMeta(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static WorldContext? load(Stream str, bool closeAfterRead = true)
    {
        using var zs = new InflaterInputStream(str, new Inflater(), 8192);
        zs.IsStreamOwner = closeAfterRead;
        var stream = new CounterInputStream(new DataInputStream(zs));
        WorldContext ctx = null;

        readHeader(stream);
        int version = stream.ReadInt();
        if (version < 6)
            throw new UnsupportedVersionException(version);
        // save.Original = str;
        ctx = new WorldContext();
        save.read(stream, ctx);
        
        return ctx;
    }

    public static void readHeader(DataInput input)
    {
        for (int i = 0; i < header.Length; i++)
        {
            if(input.ReadByte() != header[i])
                throw new IOException("Incorrect header!");
        }
    }
}

public class UnsupportedVersionException : Exception
{
    public override string Message => $"This save made in unsupported game version. Map game version:{version}";
    private readonly int version;
    public UnsupportedVersionException(int version)
    {
        this.version = version;
    }
}