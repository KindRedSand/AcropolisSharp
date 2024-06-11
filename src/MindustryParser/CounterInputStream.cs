using JavaStream;

namespace Playground;

public class CounterInputStream : DataInput
{
    private DataInputStream dis;
    public long Counter { get; private set; } = 0;

    public CounterInputStream(DataInputStream str)
    {
        dis = str;
    }
    
    
    public bool ReadBoolean()
    {
        Counter += 1;
        return dis.ReadBoolean();
    }

    public sbyte ReadByte()
    {
        Counter += 1;
        return dis.ReadByte();
    }

    public char ReadChar()
    {
        Counter += 2;
        return dis.ReadChar();
    }

    public double ReadDouble()
    {
        Counter += 8;
        return dis.ReadDouble();
    }

    public float ReadFloat()
    {
        Counter += 4;
        return dis.ReadFloat();
    }

    public void ReadFully(byte[] buffer)
    {
        Counter += buffer.Length;
        dis.ReadFully(buffer);
    }

    public void ReadFully(byte[] buffer, int offset, int count)
    {
        Counter += count;
        dis.ReadFully(buffer, offset, count);
    }

    public int Read()
    {
        Counter += 1;
        return dis.Read();
    }

    public int ReadInt()
    {
        Counter += 4;
        return dis.ReadInt();
    }

    public string ReadLine()
    {
        throw new NotImplementedException();
    }

    public long ReadLong()
    {
        Counter += 8;
        return dis.ReadLong();
    }

    public short ReadShort()
    {
        Counter += 2;
        return dis.ReadShort();
    }

    public int ReadUnsignedByte()
    {
        Counter += 1;
        return dis.ReadUnsignedByte();
    }

    public int ReadUnsignedShort()
    {
        Counter += 2;
        return dis.ReadUnsignedShort();
    }

    public string ReadUTF()
    {
        string s = dis.ReadUTF(out int c);
        Counter += c;
        return s;
    }

    public int SkipBytes(int count)
    {
        Counter += count;
        return dis.SkipBytes(count);
    }

    public long Skip(long cnt)
    {
        Counter += cnt;
        return dis.Skip(cnt);
    }

    public int Available()
    {
        throw new NotImplementedException();
    }
}