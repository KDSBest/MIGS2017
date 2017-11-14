using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class SFPSShot
    {
        public SFPSRay Ray = new SFPSRay();

        public int InterpolationOriginId;
        public int InterpolationDestinationId;

        // Percentage
        public byte InterpolationPercentage;

        public static int PacketSize
        {
            get { return SFPSVector3.PacketSize * 2 + 4 + 4 + 1; }
        }

        public bool IsValid
        {
            get { return InterpolationOriginId != InterpolationDestinationId; }
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Ray.Serialize());
            data.AddRange(BitConverter.GetBytes(InterpolationOriginId));
            data.AddRange(BitConverter.GetBytes(InterpolationDestinationId));
            data.Add(InterpolationPercentage);

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            offset = Ray.Deserialize(data, offset);
            InterpolationOriginId = BitConverter.ToInt32(data, offset);
            offset += 4;
            InterpolationDestinationId = BitConverter.ToInt32(data, offset);
            offset += 4;
            InterpolationPercentage = data[offset];
            offset++;
            return offset;
        }
    }
}
