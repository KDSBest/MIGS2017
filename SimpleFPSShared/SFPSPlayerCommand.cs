using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class SFPSPlayerCommand
    {
        public int RoomId;
        public string MyName = string.Empty;
        public SFPSPlayerState State = new SFPSPlayerState();
        public SFPSShot Shot = new SFPSShot();
        public DateTime EstimatedServerTime = DateTime.Now;

        public static int PacketSize
        {
            get { return 20 + 4 + SFPSShot.PacketSize + SFPSPlayerState.PacketSize + 8; }
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(MyName.To20Char());
            data.AddRange(BitConverter.GetBytes(RoomId));
            data.AddRange(State.Serialize());
            data.AddRange(Shot.Serialize());
            data.AddRange(BitConverter.GetBytes(EstimatedServerTime.Ticks));
            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            MyName = data.From20Char(offset);
            offset += 20;
            RoomId = BitConverter.ToInt32(data, offset);
            offset += 4;

            offset = State.Deserialize(data, offset);
            offset = Shot.Deserialize(data, offset);
            EstimatedServerTime = new DateTime(BitConverter.ToInt64(data, offset));
            offset += 8;
            return offset;
        }
    }
}
