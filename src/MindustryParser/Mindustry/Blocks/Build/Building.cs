using JavaStream;
using Playground.Mindustry.Modules;

namespace Playground.Mindustry.Blocks.Build;

public abstract class Building
{
    public float health;
    public bool enabled = true;
    public ItemModule? items;
    public PowerModule? power;
    public LiquidModule? liquids;
    public long visibleFlags;
    public float efficiency = 1;
    public float optionalEfficiency;
    public Teams team = Teams.derelict;
    public sbyte rotation;
    protected int _len;
    public void readAll(DataInput _read, sbyte revision, int len)
    {
        if (_read is CounterInputStream str)
        {
            _len = len;
            var rc = str.Counter;
            _readBase(_read);
            len -= (int)(str.Counter - rc);
            read(_read, revision, len);
            if (str.Counter - rc != _len)
                throw new InvalidDataException($"Expected lenght doesn't match! Expected {_len} but readed {str.Counter - rc}");
        }
        else
        {
            len -= readBase(_read);
            read(_read, revision, len);
        }
    }

    protected abstract void read(DataInput read, sbyte revision, int len);
    
    private int readBase(DataInput read)
    {
        var readed = 4;
        health = Math.Min(read.ReadFloat(), health);
        readed += 1;
        byte rot = (byte)read.ReadByte();
        readed += 1;
        team = (Teams)read.ReadByte();
        rotation = (sbyte)(rot & 127);
        int moduleBits = moduleBitmask();
        bool legacy = true;
        sbyte version = 0;
        if ((rot & 128) != 0) {
            readed += 1;
            version = read.ReadByte();
            if (version >= 1) {
                readed += 1;
                sbyte on = read.ReadByte();
                enabled = on == 1;
            }
            if (version >= 2) {
                readed += 1;
                moduleBits = read.ReadByte();
            }
            legacy = false;
        }
        if ((moduleBits & 1) != 0)
        {
            readed += (items ?? new ItemModule()).read(read, legacy);
        }
        if ((moduleBits & 2) != 0)
        {
            readed += (power ?? new PowerModule()).read(read, legacy);
        }
        if ((moduleBits & 4) != 0)
        {
            readed += (liquids ?? new LiquidModule()).read(read, legacy);
        }
        switch (version)
        {
            case <= 2:
                readed += 1;
                read.ReadBoolean();
                break;
            case >= 3:
                readed += 1;
                efficiency = read.ReadUnsignedByte() / 255.0F;
                readed += 1;
                optionalEfficiency = read.ReadUnsignedByte() / 255.0F;
                break;
        }

        if (version == 4) {
            readed += 8;
            visibleFlags = read.ReadLong();
        }

        return readed;
    }

    private int _readBase(DataInput read)
    {
        var readed = 4;
        health = Math.Min(read.f(), health);
        readed += 1;
        sbyte rot = read.b();
        readed += 1;
        team = (Teams)read.b();
        rotation = (sbyte)(rot & 127);
        int moduleBits = moduleBitmask();
        bool legacy = true;
        sbyte version = 0;
        if ((rot & 128) != 0) {
            readed += 1;
            version = read.b();
            if (version >= 1) {
                readed += 1;
                sbyte on = read.b();
                this.enabled = on == 1;
            }
            if (version >= 2) {
                readed += 1;
                moduleBits = read.b();
            }
            legacy = false;
        }
        if ((moduleBits & 1) != 0)
        {
            readed += (items ?? new ItemModule()).read(read, legacy);
        }
        if ((moduleBits & 2) != 0)
        {
            readed += (power ?? new PowerModule()).read(read, legacy);
        }
        if ((moduleBits & 4) != 0)
        {
            readed += (liquids ?? new LiquidModule()).read(read, legacy);
        }
        if (version <= 2)
        {
            readed += 1;
            read.ReadBoolean();
        }
        if (version >= 3) {
            readed += 1;
            efficiency = read.ub() / 255.0F;
            readed += 1;
            optionalEfficiency = read.ub() / 255.0F;
        }
        if (version == 4) {
            readed += 8;
            visibleFlags = read.l();
        }

        return readed;
    }
    
    public int moduleBitmask() 
    {
        return (items != null ? 1 : 0) | (power != null ? 2 : 0) | (liquids != null ? 4 : 0) | 8;
    }

}