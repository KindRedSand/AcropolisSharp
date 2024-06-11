using arc.util.io;
using JavaStream;
using Playground.Mindustry.Maps;

namespace Playground.Mindustry.Saves;

public class SaveFileReader
{
    public static readonly Dictionary<string, string> fallback = new()
    {
        ["dart-mech-pad"] = "legacy-mech-pad",
        ["dart-ship-pad"] = "legacy-mech-pad",
        ["javelin-ship-pad"] = "legacy-mech-pad",
        ["trident-ship-pad"] = "legacy-mech-pad",
        ["glaive-ship-pad"] = "legacy-mech-pad",
        ["alpha-mech-pad"] = "legacy-mech-pad",
        ["tau-mech-pad"] = "legacy-mech-pad",
        ["omega-mech-pad"] = "legacy-mech-pad",
        ["delta-mech-pad"] = "legacy-mech-pad",

        ["draug-factory"] = "legacy-unit-factory",
        ["spirit-factory"] = "legacy-unit-factory",
        ["phantom-factory"] = "legacy-unit-factory",
        ["wraith-factory"] = "legacy-unit-factory",
        ["ghoul-factory"] = "legacy-unit-factory-air",
        ["revenant-factory"] = "legacy-unit-factory-air",
        ["dagger-factory"] = "legacy-unit-factory",
        ["crawler-factory"] = "legacy-unit-factory",
        ["titan-factory"] = "legacy-unit-factory-ground",
        ["fortress-factory"] = "legacy-unit-factory-ground",

        ["mass-conveyor"] = "payload-conveyor",
        ["vestige"] = "scepter",
        ["turbine-generator"] = "steam-generator",

        ["rocks"] = "stone-wall",
        ["sporerocks"] = "spore-wall",
        ["icerocks"] = "ice-wall",
        ["dunerocks"] = "dune-wall",
        ["sandrocks"] = "sand-wall",
        ["shalerocks"] = "shale-wall",
        ["snowrocks"] = "snow-wall",
        ["saltrocks"] = "salt-wall",
        ["dirtwall"] = "dirt-wall",

        ["ignarock"] = "basalt",
        ["holostone"] = "dacite",
        ["holostone-wall"] = "dacite-wall",
        ["rock"] = "boulder",
        ["snowrock"] = "snow-boulder",
        ["cliffs"] = "stone-wall",
        ["craters"] = "crater-stone",
        ["deepwater"] = "deep-water",
        ["water"] = "shallow-water",
        ["sand"] = "sand-floor",
        ["slag"] = "molten-slag",

        ["cryofluidmixer"] = "cryofluid-mixer",
        ["block-forge"] = "constructor",
        ["block-unloader"] = "payload-unloader",
        ["block-loader"] = "payload-loader",
        ["thermal-pump"] = "impulse-pump",
        ["alloy-smelter"] = "surge-smelter",
        ["steam-vent"] = "rhyolite-vent",
        ["fabricator"] = "tank-fabricator",
        ["basic-reconstructor"] = "refabricator"
    };

    public static readonly Dictionary<string, string> modContentNameMap = new()
    {
        ["craters"] = "crater-stone",
        ["deepwater"] = "deep-water",
        ["water"] = "shallow-water",
        ["slag"] = "molten-slag"
    };

    public SaveFileReader()
    {
        byteOutput = new ReusableByteOutStream();
        byteOutput2 = new ReusableByteOutStream();
        byteOutputSmall = new ReusableByteOutStream();
        dataBytes = new DataOutputStream(byteOutput);
        dataBytes2 = new DataOutputStream(byteOutput2);
        dataBytesSmall = new DataOutputStream(byteOutputSmall);
    }

    protected ReusableByteOutStream byteOutput, byteOutput2;
    protected DataOutputStream dataBytes, dataBytes2;
    protected ReusableByteOutStream byteOutputSmall;
    protected DataOutputStream dataBytesSmall;
    
    protected bool chunkNested;

    protected int lastRegionLength;
    
    public virtual void region(string name, DataInput stream, Action<DataInput> cons)
    {
        try
        {
            readChunk(stream, cons);
        }
        catch (Exception e)
        {
            throw new IOException("Error reading region \"" + name + "\".", e);
        }
    }

    public virtual void region(string name, DataOutput stream, Action<DataOutput> cons)
    {
        try
        {
            writeChunk(stream, cons);
        }
        catch (Exception e)
        {
            throw new IOException("Error writing region \"" + name + "\".", e);
        }
    }

    public virtual int readChunk(DataInput input, Action<DataInput> runner)
    {
        return readChunk(input, false, runner);
    }
    
    public virtual void writeChunk(DataOutput output, Action<DataOutput> runner){
        writeChunk(output, false, runner);
    }

    /** Write a chunk of input to the stream. An integer of some length is written first, followed by the data. */
    public virtual void writeChunk(DataOutput output, bool isShort, Action<DataOutput> runner)
    {
        //TODO awful
        bool wasNested = chunkNested;
        if (!isShort)
        {
            chunkNested = true;
        }

        ReusableByteOutStream dout =
            isShort ? byteOutputSmall :
            wasNested ? byteOutput2 :
            byteOutput;
        try
        {
            //reset output position
            dout.Reset();
            //write the needed info
            runner?.Invoke(
                isShort ? dataBytesSmall :
                wasNested ? dataBytes2 :
                dataBytes
            );

            int length = (int)dout.Length;
            //write length (either int or byte) followed by the output bytes
            if (!isShort)
            {
                output.WriteInt(length);
            }
            else
            {
                if (length > 65535)
                {
                    throw new IOException("Byte write length exceeded: " + length + " > 65535");
                }

                output.WriteShort(length);
                
            }

            output.Write(dout.GetBytes(), 0, length);
        }
        finally
        {
            chunkNested = wasNested;
        }
    }


    public virtual int readChunk(DataInput input, bool isShort, Action<DataInput>? runner)
    {
        int length = isShort ? input.ReadUnsignedShort() : input.ReadInt();
        lastRegionLength = length;
        runner?.Invoke(input);
        return length;
    }
    
    public virtual int readSkippableChunk(DataInput input, bool isShort, Action<DataInput, int>? runner)
    {
        int length = isShort ? input.ReadUnsignedShort() : input.ReadInt();
        lastRegionLength = length;
        runner?.Invoke(input, length);
        return length;
    }

    public void skipChunk(DataInput input){
        skipChunk(input, false);
    }

    /** Skip a chunk completely, discarding the bytes. */
    public void skipChunk(DataInput input, bool isShort)
    {
        int length = readChunk(input, isShort, x => {});
        int skipped = input.SkipBytes(length);
        if (length != skipped)
        {
            throw new IOException("Could not skip bytes. Expected length: " + length + "; Actual length: " + skipped);
        }
    }

    public void writeStringMap(DataOutput stream, Dictionary<string, string> map)
    {
        stream.WriteShort(map.Count);
        foreach ((string key, string value) in map)
        {
            stream.WriteUTF(key);
            stream.WriteUTF(value);
        }
    }

    public Dictionary<string, string> readStringMap(DataInput stream)
    {
        Dictionary<string, string> map = new ();
        short size = stream.ReadShort();
        for (int i = 0; i < size; i++)
        {
            map.Add(stream.ReadUTF(), stream.ReadUTF());
        }

        return map;
    }

    public virtual void read(DataInput stream , WorldContext context)
    {
        
    }

    public virtual void write(DataOutputStream stream)
    {
        
    }

}

// public interface IORunner<T>{
//     void accept(T stream);
// }

public interface CustomChunk
{
    void write(DataOutput stream);
    void read(DataInput stream);

    /** @return whether this chunk is enabled at all */
    bool shouldWrite()
    {
        return true;
    }

    /** @return whether this chunk should be written to connecting clients (default true) */
    bool writeNet()
    {
        return true;
    }
}