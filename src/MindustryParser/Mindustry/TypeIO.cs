using JavaStream;

using System.Drawing;

namespace Playground.Mindustry
{
    public static class TypeIO
    {

        public static void WriteObject(DataOutput write, object? obj)
        {
            switch (obj)
            {
                case null:
                    write.WriteByte(0);
                    return;
                case int _i:
                    write.WriteByte((byte)1);
                    write.WriteInt(_i);
                    break;
                case long _l:
                    write.WriteByte((byte)2);
                    write.WriteLong(_l);
                    break;
                case float _f:
                    write.WriteByte((byte)3);
                    write.WriteFloat(_f);
                    break;
                case string _s:
                    write.WriteByte((byte)4);
                    writeString(write, _s);
                    break;
                case Content content:
                    write.WriteByte((byte)5);
                    write.WriteByte((byte)content.type);
                    write.WriteShort(content.id);
                    break;
                case List<int> arr:
                {
                    write.WriteByte((byte)6);
                    write.WriteShort((short)arr.Count);
                    foreach (int t in arr)
                    {
                        write.WriteInt(t);
                    }

                    break;
                }
                case Point2 p:
                    write.WriteByte((byte)7);
                    write.WriteInt(p.X);
                    write.WriteInt(p.Y);
                    break;
                case Point2[] ps:
                {
                    write.WriteByte((byte)8);
                    write.WriteByte(ps.Length);
                    foreach (var point in ps)
                    {
                        write.WriteInt(PackPoint(point));
                    }

                    break;
                }
                case UnlockableContent unlockableContent:
                    write.WriteByte(9);
                    write.WriteByte((byte)unlockableContent.type);
                    write.WriteShort(unlockableContent.id);
                    break;
                case bool _b:
                    write.WriteByte((byte)10);
                    write.WriteBoolean(_b);
                    break;
                case double d:
                    write.WriteByte((byte)11);
                    write.WriteDouble(d);
                    break;
                //else if (obj is Building b){
                //    write.WriteByte(12);
                //    write.WriteInt(b.pos());
                //}else if (obj is BuildingBox b){
                //    write.WriteByte(12);
                //    write.WriteInt(b.pos);
                //}
                case LAccess l:
                    write.WriteByte((byte)13);
                    write.WriteShort((short)l);
                    break;
                case byte[] bs:
                {
                    write.WriteByte((byte)14);
                    write.WriteInt(bs.Length);
                    foreach (byte b in bs)
                    {
                        write.WriteByte(b);
                    }

                    break;
                }
                case bool[] bools:
                {
                    write.WriteByte(16);
                    write.WriteInt(bools.Length);
                    foreach (var b in bools)
                    {
                        write.WriteBoolean(b);
                    }

                    break;
                }
                //else if (obj is Unit u){
                //    write.WriteByte(17);
                //    write.WriteInt(u.id);
                //}else if (obj is UnitBox u){
                //    write.WriteByte(17);
                //    write.WriteInt(u.id);
                //}
                case Vec2[] vecs:
                {
                    write.WriteByte(18);
                    write.WriteShort(vecs.Length);
                    foreach (Vec2 v in vecs)
                    {
                        write.WriteFloat(v.x);
                        write.WriteFloat(v.y);
                    }

                    break;
                }
                case Vec2 v:
                    write.WriteByte((byte)19);
                    write.WriteFloat(v.x);
                    write.WriteFloat(v.y);
                    break;
                case Teams team:
                    write.WriteByte((byte)20);
                    write.WriteByte((byte)team);
                    break;
                case int[] ints:
                {
                    write.WriteByte((byte)21);
                    foreach (var i in ints)
                    {
                        write.WriteInt(i);
                    }

                    break;
                }
                case object?[] objs:
                {
                    write.WriteByte((byte)22);
                    write.WriteInt(objs.Length);
                    foreach (var ob in objs)
                    {
                        WriteObject(write, ob);
                    }

                    break;
                }
            }

            //else if (obj is UnitCommand command){
            //    write.WriteByte(23);
            //    write.s(command.id);
            //}
        }

        public static object? ReadObject(DataInput read, bool boxed = false)
        {
            var type = read.ReadByte();
            switch (type)
            {
                case 0:
                    return null;
                case 1:
                    return read.ReadInt();
                case 2:
                    return read.ReadLong();
                case 3:
                    return read.ReadFloat();
                case 4:
                    return readString(read);
                case 5:
                    //Console.WriteLine("content.getByID");
                    return new Content((ContentType)read.ReadByte(), read.ReadShort());
                case 6:
                {
                    short lenght = read.ReadShort();
                    List<int> arr = new();
                    for (int i = 0; i < lenght; i++)
                    {
                        arr.Add(read.ReadInt());
                    }
                    return arr;
                }
                case 7:
                    return new Point2((short)read.ReadInt(), (short)read.ReadInt());
                case 8:
                {
                    sbyte len = read.ReadByte();
                    Point2[] arr = new Point2[len];
                    for (int i = 0; i < len; i++)
                    {
                        arr[i] = UnpackPoint(read.ReadInt());
                    }
                    return arr;
                }
                case 9:
                    Console.WriteLine("content.UnlockableContent.GetByID.technode");
                    return new UnlockableContent((ContentType)read.ReadByte(), read.ReadShort());
                case 10:
                    return read.ReadBoolean();
                case 11:
                    return read.ReadDouble();
                case 12:
                    Console.WriteLine("world.build");
                    return read.ReadInt();
                case 13:
                    return (LAccess) read.ReadShort();
                case 14:
                {
                    var len = read.ReadInt();
                    var arr = new sbyte[len];
                    for (int i = 0; i < len; i++)
                    {
                        arr[i] = read.ReadByte();
                    }

                    return arr;
                }
                case 15:
                    Console.WriteLine("unit command");
                    read.ReadByte();
                    return null;
                case 16:
                {
                    var len = read.ReadInt();
                    var arr = new bool[len];
                    for (int i = 0; i < len; i++)
                    {
                        arr[i] = read.ReadBoolean();
                    }

                    return arr;
                }
                case 17:
                    return read.ReadInt();
                case 18:
                {
                    sbyte len = read.ReadByte();
                    Vec2[] arr = new Vec2[len];
                    for (int i = 0; i < len; i++)
                    {
                        arr[i] = new Vec2(read.ReadFloat(), read.ReadFloat());
                    }
                    return arr;
                }
                case 19:
                    return new Vec2(read.ReadFloat(), read.ReadFloat());
                case 20:
                    return (Teams) read.ReadUnsignedByte();
                case 21:
                {
                    short length = read.ReadShort();
                    int[] arr = new int[length];
                    for (int i = 0; i < length; i++)
                    {
                        arr[i] = read.ReadInt();
                    }
                    return arr;
                }
                case 22:
                {
                    var length = read.ReadInt();
                    var arr = new object?[length];
                    for (int i = 0; i < length; i++)
                    {
                        arr[i] = ReadObject(read);
                    }
                    return arr;
                }
                case 23:
                    Console.WriteLine("unit command");
                    return read.ReadUnsignedShort();
                default:
                    return null;
            }
        }


        private static string? readString(DataInput read)
        {
            var exist = read.ReadByte();
            if (exist != 0)
            {
                return read.ReadUTF();
            }

            return null;
        }

        private static void writeString(DataOutput write, string? s)
        {
            if (s != null)
            {
                write.WriteByte(1);
                write.WriteUTF(s);
            }
            else
            {
                write.WriteByte(0);
            }
        }


        public static Point2 UnpackPoint(int pos)
        {
            return new Point2((short)(pos >>> 16), (short)(pos & 0xFFFF));
        }
        
        public static int PackPoint(Point2 p)
        {
            return (((short)p.X) << 16) | (((short)p.Y) & 0xFFFF);
        }

        
        //Discard everything, we just want to read next block
        public static void ReadPlans(DataInput read){
            short reqamount = read.s();
            if(reqamount == -1){
                return;
            }

            for(int i = 0; i < reqamount; i++){
                readPlan(read);
            }
        }
    
    
        public static void readPlan(DataInput read){
            sbyte type = read.b();
            int position = read.i();

            if(type == 1){ //remove
            }else{ //place
                short block = read.s();
                sbyte rotation = read.b();
                bool hasConfig = read.b() == 1;
                object? config = ReadObject(read);
               
            }
        }
        
        public static byte readCommand(DataInput read){
            return read.ub();
        }
        
        public static Vec2? readVecNullable(DataInput read){
            float x = read.f(), y = read.f();
            return float.IsNaN(x) || float.IsNaN(y) ? null : new Vec2(x, y);
        }
    }

   
    
    
    public record Content(ContentType type, short id);
    public record UnlockableContent(ContentType type, short id);

    public record Vec2(float x, float y);

    public record ItemPrice(string name, int count = 1);

    public record Block(string name/*, ItemPrice[] price*/, int size = 1);

    public record TileData(Block block, short x, short y, object? config, sbyte rotation, Block? floor = null, Block? overlay = null, Teams team = Teams.derelict);

    public struct Point2(short x, short y)
    {
        public short X = x, Y = y;

        public static Point2 operator  +(Point2 f, Point2 s)
        {
            return new Point2((short)(f.X + s.X), (short)(f.Y + s.Y));
        }
    }
}
