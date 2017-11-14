using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Messages
{
    public class LockstepState
    {
        public int Frame = 0;
        public Dictionary<byte, Vector2> ClickPosition = new Dictionary<byte, Vector2>();

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(Frame));
            data.Add((byte)ClickPosition.Count);
            foreach (var clickPos in ClickPosition)
            {
                data.Add(clickPos.Key);
                data.AddRange(BitConverter.GetBytes(clickPos.Value.x));
                data.AddRange(BitConverter.GetBytes(clickPos.Value.y));
            }

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            ClickPosition.Clear();
            Frame = BitConverter.ToInt32(data, offset);
            int internalOffset = 4;
            int count = data[offset+internalOffset];
            internalOffset++;

            for (int i = 0; i < count; i++)
            {
                ClickPosition.Add(data[offset + internalOffset], new Vector2(BitConverter.ToSingle(data, offset + internalOffset + 1), BitConverter.ToSingle(data, offset + internalOffset + 5)));
                internalOffset += 9;
            }

            return offset + internalOffset;
        }

        public static List<LockstepState> DeserializeAll(byte[] data)
        {
            List<LockstepState> result = new List<LockstepState>();
            int offset = 0;

            while (offset < data.Length)
            {
                var msg = new LockstepState();
                offset = msg.Deserialize(data, offset);
                result.Add(msg);
            }
            return result;
        }
    }
}
