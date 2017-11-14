using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimpleFPSShared
{
    public class SFPSPlayerState
    {
        public float Rotation;
        public SFPSVector2 Position = new SFPSVector2();

        public static int PacketSize
        {
            get { return 4 + SFPSVector2.PacketSize; }
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Position.Serialize());
            data.AddRange(BitConverter.GetBytes(Rotation));

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            offset = Position.Deserialize(data, offset);
            Rotation = BitConverter.ToSingle(data, offset);
            offset += 4;
            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0} - Rot{1}", Position, Rotation);
        }
    }
}
