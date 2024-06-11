using System.Text;
//Source: https://github.com/cping/LGame/blob/master/C%2523/Loon2Unity/Loon.Java
namespace JavaStream
{
    public interface DataInput
    {
        bool ReadBoolean();
        sbyte ReadByte();
        char ReadChar();
        double ReadDouble();
        float ReadFloat();
        void ReadFully(byte[] buffer);
        void ReadFully(byte[] buffer, int offset, int count);
        int Read();
        int ReadInt();
        string ReadLine();
        long ReadLong();
        short ReadShort();
        int ReadUnsignedByte();
        int ReadUnsignedShort();
        string ReadUTF();
        int SkipBytes(int count);
        long Skip(long cnt);

        int Available();
        short s() => ReadShort();
        sbyte b() => ReadByte();
        byte ub() => (byte)ReadUnsignedByte();
        long l() => ReadLong();
        float f() => ReadFloat();
        int i() => ReadInt();
        double d() => ReadDouble();
    }

    public class DataInputStream : DataInput
    {

        private byte[] buff;
        private int limit = -1;
        private long marked = -1L;
        private Stream stream;

        public DataInputStream(Stream stream)
        {
            this.stream = stream;
            this.buff = new byte[8];
        }

        public int Available()
        {
            return (int)(this.stream.Length - this.stream.Position);
        }

        public static string ConvertUTF8WithBuf(byte[] buf, char[] outb, int offset, int utfSize)
        {
            int num = 0;
            int index = 0;
            while (num < utfSize)
            {
                if ((outb[index] = (char)buf[offset + num++]) < '\x0080')
                {
                    index++;
                }
                else
                {
                    int num3;
                    if (((num3 = outb[index]) & '\x00e0') == 0xc0)
                    {
                        if (num >= utfSize)
                        {
                            throw new Exception("K0062");
                        }
                        int num4 = buf[num++];
                        if ((num4 & 0xc0) != 0x80)
                        {
                            throw new Exception("K0062");
                        }
                        outb[index++] = (char)(((num3 & 0x1f) << 6) | (num4 & 0x3f));
                    }
                    else
                    {
                        if ((num3 & 240) != 0xe0)
                        {
                            throw new Exception("K0065");
                        }
                        if ((num + 1) >= utfSize)
                        {
                            throw new Exception("K0063");
                        }
                        int num5 = buf[num++];
                        int num6 = buf[num++];
                        if (((num5 & 0xc0) != 0x80) || ((num6 & 0xc0) != 0x80))
                        {
                            throw new Exception("K0064");
                        }
                        outb[index++] = (char)((((num3 & 15) << 12) | ((num5 & 0x3f) << 6)) | (num6 & 0x3f));
                    }
                    continue;
                }
            }
            return new string(outb, 0, index);
        }

        private string DecodeUTF(int utfSize)
        {
            return DecodeUTF(utfSize, this);
        }

        private static string DecodeUTF(int utfSize, DataInput stream)
        {
            byte[] buffer = new byte[utfSize];
            char[] outb = new char[utfSize];
            stream.ReadFully(buffer, 0, utfSize);
            return ConvertUTF8WithBuf(buffer, outb, 0, utfSize);
        }

        public void Mark(int limit)
        {
            this.marked = this.stream.Position;
            this.limit = limit;
        }

        public int Read()
        {
            return stream.ReadByte();
        }

        public int Read(byte[] buffer)
        {
            return this.stream.Read(buffer, 0, buffer.Length);
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            return this.stream.Read(buffer, offset, length);
        }

        public int Limit()
        {
            return this.limit;
        }

        public bool ReadBoolean()
        {
            int num = this.stream.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            return (num != 0);
        }

        public sbyte ReadByte()
        {
            int num = this.stream.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            return (sbyte)num;
        }

        public char ReadChar()
        {
            if (this.ReadToBuff(2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (char)(((this.buff[0] & 0xff) << 8) | (this.buff[1] & 0xff));
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(this.ReadLong());
        }

        public float ReadFloat()
        {
            return (float)BitConverter.DoubleToInt64Bits((double)this.ReadInt());
        }

        public void ReadFully(byte[] buffer)
        {
            this.ReadFully(buffer, 0, buffer.Length);
        }

        public void ReadFully(sbyte[] buffer)
        {
            this.ReadFully(buffer, 0, buffer.Length);
        }

        public void ReadFully(byte[] buffer, int offset, int length)
        {
            if (length < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (length != 0)
            {
                if (this.stream == null)
                {
                    throw new NullReferenceException("Stream is null");
                }
                if (buffer == null)
                {
                    throw new NullReferenceException("buffer is null");
                }
                if ((offset < 0) || (offset > (buffer.Length - length)))
                {
                    throw new IndexOutOfRangeException();
                }
                while (length > 0)
                {
                    int num = this.stream.Read(buffer, offset, length);
                    if (num == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += num;
                    length -= num;
                }
            }
        }

        public void ReadFully(sbyte[] buffer, int offset, int length)
        {
            int num = offset;
            if (length < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (length != 0)
            {
                if (this.stream == null)
                {
                    throw new NullReferenceException("Stream is null");
                }
                if (buffer == null)
                {
                    throw new NullReferenceException("buffer is null");
                }
                if ((offset < 0) || (offset > (buffer.Length - length)))
                {
                    throw new IndexOutOfRangeException();
                }
                byte[] buffer2 = new byte[buffer.Length];
                while (length > 0)
                {
                    int num2 = this.stream.Read(buffer2, offset, length);
                    if (num2 == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += num2;
                    length -= num2;
                }
                for (int i = num; i < buffer.Length; i++)
                {
                    buffer[i] = (sbyte)buffer2[i];
                }
            }
        }

        public int ReadInt()
        {
            if (this.ReadToBuff(4) < 0)
            {
                throw new EndOfStreamException();
            }
            return (((((this.buff[0] & 0xff) << 0x18) | ((this.buff[1] & 0xff) << 0x10)) | ((this.buff[2] & 0xff) << 8)) | (this.buff[3] & 0xff));
        }

        public string ReadLine()
        {
            int num = 0;
            int num2 = 0;
            StringBuilder builder = new StringBuilder(80);
            bool flag = false;
            while (true)
            {
                do
                {
                    num = this.stream.ReadByte();
                    switch (num)
                    {
                        case -1:
                            if ((builder.Length == 0) && !flag)
                            {
                                return null;
                            }
                            return builder.ToString();

                        case 10:
                            return builder.ToString();
                    }
                }
                while (num2 == 13);
                builder.Append((char)num);
            }
        }

        public long ReadLong()
        {
            if (this.ReadToBuff(8) < 0)
            {
                throw new EndOfStreamException();
            }
            int num = ((((this.buff[0] & 0xff) << 0x18) | ((this.buff[1] & 0xff) << 0x10)) | ((this.buff[2] & 0xff) << 8)) | (this.buff[3] & 0xff);
            int num2 = ((((this.buff[4] & 0xff) << 0x18) | ((this.buff[5] & 0xff) << 0x10)) | ((this.buff[6] & 0xff) << 8)) | (this.buff[7] & 0xff);
            return (long)(((num & 0xffffffffL) << 0x20) | (num2 & 0xffffffffL));
        }

        public short ReadShort()
        {
            if (this.ReadToBuff(2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (short)(((this.buff[0] & 0xff) << 8) | (this.buff[1] & 0xff));
        }

        private int ReadToBuff(int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int num2 = this.stream.Read(this.buff, offset, count);
                if (num2 == 0)
                {
                    return num2;
                }
                offset += num2;
            }
            return offset;
        }

        public int ReadUnsignedByte()
        {
            int num = this.stream.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            return num;
        }

        public int ReadUnsignedShort()
        {
            if (this.ReadToBuff(2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (ushort)(((this.buff[0] & 0xff) << 8) | (this.buff[1] & 0xff));
        }

        public string ReadUTF()
        {
            return this.DecodeUTF(this.ReadUnsignedShort());
        }
        
        public string ReadUTF(out int count)
        {
            count = this.ReadUnsignedShort();
            return this.DecodeUTF(count);
        }

        public static string ReadUTF(DataInput stream)
        {
            return DecodeUTF(stream.ReadUnsignedShort(), stream);
        }

        public void Reset()
        {
            if (this.marked > -1L)
            {
                this.stream.Position = this.marked;
            }
            else
            {
                this.stream.Position = 0L;
            }
        }

        public long Skip(long cnt)
        {
            long n = cnt;
            while (n > 0)
            {
                if (Read() == -1)
                    return cnt - n;
                n--;
            }
            return cnt - n;
        }

        public int SkipBytes(int count)
        {
            int num = 0;
            while (num < count)
            {
                this.stream.ReadByte();
                num++;
            }
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            return num;
        }

        public void Close()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
        }

    }



    public interface DataOutput
    {
        void Flush();

        void Write(sbyte[] buffer);

        void Write(int oneByte);

        void Write(byte[] buffer, int offset, int count);

        void Write(sbyte[] buffer, int offset, int count);

        void WriteBoolean(bool val);

        void WriteByte(int val);

        void WriteBytes(string str);

        void WriteChar(char oneByte);

        void WriteChars(string str);

        void WriteDouble(double val);

        void WriteFloat(float val);

        void WriteInt(int val);

        void WriteLong(long val);

        void WriteShort(int val);

        void WriteUTF(string str);
    }

    public class DataOutputStream : DataOutput
    {
        private sbyte[] buff;
        private Stream stream;
        protected int written;

        public DataOutputStream(Stream stream)
        {
            this.stream = stream;
            this.buff = new sbyte[8];
        }

        public int Available()
        {
            return (int)(this.stream.Length - this.stream.Position);
        }

        private long CountUTFBytes(string str)
        {
            int num = 0;
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                int num4 = str[i];
                if ((num4 > 0) && (num4 <= 0x7f))
                {
                    num++;
                }
                else if (num4 <= 0x7ff)
                {
                    num += 2;
                }
                else
                {
                    num += 3;
                }
            }
            return (long)num;
        }

        public void Flush()
        {
            this.stream.Flush();
        }

        public int Size()
        {
            if (this.written < 0)
            {
                this.written = 0x7fffffff;
            }
            return this.written;
        }

        public void Write(sbyte[] buffer)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("K0047");
            }
            foreach (byte num in buffer)
            {
                this.stream.WriteByte(num);
            }
        }

        public void Write(int oneByte)
        {
            byte num = (byte)oneByte;
            this.stream.WriteByte(num);
            this.written++;
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("K0047");
            }
            this.stream.Write(buffer, offset, count);
            this.written += count;
        }

        public void Write(sbyte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("K0047");
            }
            byte[] buffer2 = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer2[i] = (byte)buffer[i];
            }
            this.stream.Write(buffer2, offset, count);
            this.written += count;
        }

        public void WriteBoolean(bool val)
        {
            this.stream.WriteByte(val ? ((byte)1) : ((byte)0));
            this.written++;
        }

        public void WriteByte(int val)
        {
            this.stream.WriteByte((byte)val);
            this.written++;
        }

        public void WriteBytes(string str)
        {
            if (str.Length != 0)
            {
                sbyte[] buffer = new sbyte[str.Length];
                for (int i = 0; i < str.Length; i++)
                {
                    buffer[i] = (sbyte)str[i];
                }
                this.Write(buffer);
                this.written += buffer.Length;
            }
        }

        public void WriteChar(char val)
        {
            this.buff[0] = (sbyte)(val >> 8);
            this.buff[1] = (sbyte)val;
            this.Write(this.buff, 0, 2);
            this.written += 2;
        }

        public void WriteChars(string str)
        {
            sbyte[] buffer = new sbyte[str.Length * 2];
            for (int i = 0; i < str.Length; i++)
            {
                int index = (i == 0) ? i : (i * 2);
                buffer[index] = (sbyte)(str[i] >> 8);
                buffer[index + 1] = (sbyte)str[i];
            }
            this.Write(buffer);
            this.written += buffer.Length;
        }

        public void WriteDouble(double val)
        {
            this.WriteLong(BitConverter.DoubleToInt64Bits(val));
        }

        public void WriteFloat(float val)
        {
            this.WriteInt((int)BitConverter.DoubleToInt64Bits((double)((int)val)));
        }

        public void WriteInt(int val)
        {
            this.buff[0] = (sbyte)(val >> 0x18);
            this.buff[1] = (sbyte)(val >> 0x10);
            this.buff[2] = (sbyte)(val >> 8);
            this.buff[3] = (sbyte)val;
            this.Write(this.buff, 0, 4);
            this.written += 4;
        }

        public void WriteLong(long val)
        {
            this.buff[0] = (sbyte)(val >> 0x38);
            this.buff[1] = (sbyte)(val >> 0x30);
            this.buff[2] = (sbyte)(val >> 40);
            this.buff[3] = (sbyte)(val >> 0x20);
            this.buff[4] = (sbyte)(val >> 0x18);
            this.buff[5] = (sbyte)(val >> 0x10);
            this.buff[6] = (sbyte)(val >> 8);
            this.buff[7] = (sbyte)val;
            this.Write(this.buff, 0, 8);
            this.written += 8;
        }

        public void WriteShort(int val)
        {
            this.buff[0] = (sbyte)(val >> 8);
            this.buff[1] = (sbyte)val;
            this.Write(this.buff, 0, 2);
            this.written += 2;
        }

        public void WriteUTF(string str)
        {
            long count = this.CountUTFBytes(str);
            if (count > 0xffffL)
            {
                throw new Exception("K0068");
            }
            this.WriteShort((int)count);
            this.WriteUTFBytes(str, count);
        }

        private void WriteUTFBytes(string str, long count)
        {
            int num = (int)count;
            int length = str.Length;
            sbyte[] buffer = new sbyte[num];
            int num3 = 0;
            for (int i = 0; i < length; i++)
            {
                int num5 = str[i];
                if ((num5 > 0) && (num5 <= 0x7f))
                {
                    buffer[num3++] = (sbyte)num5;
                }
                else if (num5 <= 0x7ff)
                {
                    buffer[num3++] = (sbyte)(0xc0 | (0x1f & (num5 >> 6)));
                    buffer[num3++] = (sbyte)(0x80 | (0x3f & num5));
                }
                else
                {
                    buffer[num3++] = (sbyte)(0xe0 | (15 & (num5 >> 12)));
                    buffer[num3++] = (sbyte)(0x80 | (0x3f & (num5 >> 6)));
                    buffer[num3++] = (sbyte)(0x80 | (0x3f & num5));
                }
            }
            this.Write(buffer, 0, num3);
        }

        public void Close()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
        }

    }



}

namespace arc.util.io
{

    public class ReusableByteOutStream : MemoryStream
    {
        public ReusableByteOutStream(int capacity) : base(capacity)
        {
            
        }

        public ReusableByteOutStream()
        {
            
        }

        public byte[] GetBytes() => this.GetBuffer();

        public void Reset()
        {
            Seek(0, SeekOrigin.Begin);
        }
    }
    
}