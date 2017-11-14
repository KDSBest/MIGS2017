using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class SFPSVector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public SFPSVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public SFPSVector2(SFPSVector2 vec) : this(vec.X, vec.Y)
        {
            
        }

        public SFPSVector2()
        {
            
        }

        public static int PacketSize
        {
            get { return 8; }
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(X));
            data.AddRange(BitConverter.GetBytes(Y));

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            X = BitConverter.ToSingle(data, offset);
            Y = BitConverter.ToSingle(data, offset + 4);

            return offset + 8;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", X, Y);
        }

        public static double Distance(SFPSVector2 value1, SFPSVector2 value2)
        {
            double v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return (double)Math.Sqrt((v1 * v1) + (v2 * v2));
        }
        public static double Dot(SFPSVector2 value1, SFPSVector2 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }
        public override bool Equals(object obj)
        {
            if (obj is SFPSVector2)
            {
                return Equals((SFPSVector2)this);
            }

            return false;
        }

        public bool Equals(SFPSVector2 other)
        {
            return (X == other.X) && (Y == other.Y);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public double Length()
        {
            return (double)Math.Sqrt((X * X) + (Y * Y));
        }

        public double LengthSquared()
        {
            return (X * X) + (Y * Y);
        }

        public void Normalize()
        {
            float val = 1.0f / (float)Math.Sqrt((X * X) + (Y * Y));
            X *= val;
            Y *= val;
        }

        public static SFPSVector2 Normalize(SFPSVector2 oldValue)
        {
            float val = 1.0f / (float)Math.Sqrt((oldValue.X * oldValue.X) + (oldValue.Y * oldValue.Y));
            SFPSVector2 value = new SFPSVector2(oldValue);
            value.X *= val;
            value.Y *= val;
            return value;
        }

        public static SFPSVector2 operator -(SFPSVector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static SFPSVector2 operator -(SFPSVector2 value1, SFPSVector2 value2)
        {
            return new SFPSVector2(value1.X - value2.X, value1.Y - value2.Y);
        }
        public static SFPSVector2 operator +(SFPSVector2 value1, SFPSVector2 value2)
        {
            return new SFPSVector2(value1.X + value2.X, value1.Y + value2.Y);
        }

        public static SFPSVector2 operator *(SFPSVector2 value1, float value2)
        {
            return new SFPSVector2(value1.X * value2, value1.Y * value2);
        }

        public static bool operator ==(SFPSVector2 value1, SFPSVector2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }


        public static bool operator !=(SFPSVector2 value1, SFPSVector2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }
    }
}
