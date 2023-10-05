using System;
using System.Text;
using LibMMD.Reader;
using UnityEngine;
using mmd2timeline;

namespace LibMMD.Util
{
    public static class MmdReaderUtil2
    {
        public static string ReadStringFixedLength(BufferBinaryReader reader, int length, Encoding encoding=null)
        {
            if (length < 0)
            {
                throw new MmdFileParseException("pmx string length is negative");
            }
            if (length == 0)
            {
                return "";
            }
            var bytes = reader.ReadBytes(length);
            string str = null;
            if(encoding!=null)
                str = encoding.GetString(bytes);
            else
                str = ToEncoding.ToUnicode(bytes);
            //var str = encoding.GetString(bytes);
            var end = str.IndexOf("\0", StringComparison.Ordinal);
            if (end >= 0)
            {
                str = str.Substring(0, end);
            }
            return str;
        }

        public static string ReadSizedString(BufferBinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadInt32();
            return ReadStringFixedLength(reader, length, encoding);
        }

        public static Vector4 ReadVector4(BufferBinaryReader reader)
        {
            var ret = new Vector4();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            ret[2] = MathUtil.NanToZero(reader.ReadSingle());
            ret[3] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static Quaternion ReadQuaternion(BufferBinaryReader reader)
        {
            var ret = new Quaternion();
            ret.x = MathUtil.NanToZero(reader.ReadSingle());
            ret.y = MathUtil.NanToZero(reader.ReadSingle());
            ret.z = MathUtil.NanToZero(reader.ReadSingle());
            ret.w = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }
        
        public static Vector3 ReadVector3(BufferBinaryReader reader)
        {
            var ret = new Vector3();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            ret[2] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }
        
        public static Vector2 ReadVector2(BufferBinaryReader reader)
        {
            var ret = new Vector2();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static int ReadIndex(BufferBinaryReader reader, int size)
        {
            switch (size)
            {
                case 1:
                    return reader.ReadSByte();
                case 2:
                    return reader.ReadUInt16();
                case 4:
                    return reader.ReadInt32();
                default:
                    throw new MmdFileParseException("invalid index size: " + size);
            }
        }

        public static Color ReadColor(BufferBinaryReader reader, bool readA)
        {
            var ret = new Color
            {
                r = reader.ReadSingle(),
                g = reader.ReadSingle(),
                b = reader.ReadSingle(),
                a = readA ? reader.ReadSingle() : 1.0f
            };
            return ret;
        }
        
        //public static bool Eof(BinaryReader binaryReader)
        //{
        //    var bs = binaryReader.BaseStream;
        //    return (bs.Position == bs.Length);
        //}

    }
}