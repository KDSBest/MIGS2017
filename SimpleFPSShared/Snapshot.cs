using System;
using System.Collections.Generic;

namespace SimpleFPSShared
{
    public class Snapshot
    {
        public DateTime ServerTime = DateTime.Now;
        public Dictionary<string, SFPSPlayerState> Player = new Dictionary<string, SFPSPlayerState>();

        public Dictionary<string, List<SFPSShot>> Shots = new Dictionary<string, List<SFPSShot>>();

        public int Id;

        public bool HasHit = false;

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(ServerTime.Ticks));
            data.AddRange(BitConverter.GetBytes(Player.Count));
            foreach (var player in Player)
            {
                data.AddRange(player.Key.To20Char());
                data.AddRange(player.Value.Serialize());
            }
            data.AddRange(BitConverter.GetBytes(Shots.Count));
            foreach (var shotKeyValue in Shots)
            {
                data.AddRange(shotKeyValue.Key.To20Char());
                data.AddRange(BitConverter.GetBytes(shotKeyValue.Value.Count));
                foreach (var shot in shotKeyValue.Value)
                {
                    data.AddRange(shot.Serialize());
                }
            }

            data.AddRange(BitConverter.GetBytes(Id));
            data.Add(HasHit ? (byte)1 : (byte)0);

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            ServerTime = new DateTime(BitConverter.ToInt64(data, offset));
            offset += 8;

            int pCount = BitConverter.ToInt32(data, offset);
            offset += 4;
            for (int i = 0; i < pCount; i++)
            {
                string key = data.From20Char(offset);
                offset += 20;
                var state = new SFPSPlayerState();
                offset = state.Deserialize(data, offset);
                Player.Add(key, state);
            }

            int sCount = BitConverter.ToInt32(data, offset);
            offset += 4;
            Shots = new Dictionary<string, List<SFPSShot>>();

            for (int i = 0; i < sCount; i++)
            {
                string key = data.From20Char(offset);
                offset += 20;
                Shots.Add(key, new List<SFPSShot>());
                int playerSCount = BitConverter.ToInt32(data, offset);
                offset += 4;
                for (int ii = 0; ii < playerSCount; ii++)
                {
                    var shot = new SFPSShot();
                    offset = shot.Deserialize(data, offset);
                    Shots[key].Add(shot);
                }
            }

            Id = BitConverter.ToInt32(data, offset);
            offset += 4;
            HasHit = data[offset] != 0;
            offset++;

            return offset;
        }

        public Snapshot CopySnapshotForPlayer(int newId, bool hasHit)
        {
            Snapshot newSnapshot = new Snapshot();
            newSnapshot.Deserialize(this.Serialize(), 0);
            newSnapshot.Id = newId;
            newSnapshot.ServerTime = DateTime.Now;
            newSnapshot.HasHit = hasHit;
            return newSnapshot;
        }
    }
}
