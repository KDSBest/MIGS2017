using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class SFPSVector3
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float Z { get; set; }

        public static int PacketSize
        {
            get { return 12; }
        }


        public SFPSVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SFPSVector3(SFPSVector3 vec) : this(vec.X, vec.Y, vec.Z)
        {

        }

        public SFPSVector3()
        {

        }

        public float MagnitudeSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public SFPSVector3 Normalize()
        {
            float num = this.Magnitude();
            if (num > 1E-05f)
            {
                return this / num;
            }

            return new SFPSVector3();
        }

        public static float Dot(SFPSVector3 lhs, SFPSVector3 rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
        }

        public static SFPSVector3 operator *(SFPSVector3 a, float d)
        {
            return new SFPSVector3(a.X / d, a.Y / d, a.Z / d);
        }

        public static SFPSVector3 operator /(SFPSVector3 a, float d)
        {
            return new SFPSVector3(a.X / d, a.Y / d, a.Z / d);
        }

        public static SFPSVector3 operator +(SFPSVector3 a, SFPSVector3 b)
        {
            return new SFPSVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static SFPSVector3 operator -(SFPSVector3 a, SFPSVector3 b)
        {
            return new SFPSVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(X));
            data.AddRange(BitConverter.GetBytes(Y));
            data.AddRange(BitConverter.GetBytes(Z));

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            X = BitConverter.ToSingle(data, offset);
            Y = BitConverter.ToSingle(data, offset + 4);
            Z = BitConverter.ToSingle(data, offset + 8);

            return offset + 12;
        }
    }
}
