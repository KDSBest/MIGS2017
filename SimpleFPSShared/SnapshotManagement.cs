using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class SnapshotManagement
    {
        public const int MAX_SNAPSHOTS = 20;
        public int NewSnapshotId = 0;

        public TimeSpan ClientOffsetToServer = TimeSpan.Zero;
        public List<Snapshot> Snapshots = new List<Snapshot>();
        public bool HasHit;

        public void GenerateFromMasterSnapshot(Snapshot snapshot)
        {
            NewSnapshotId++;
            var newSnapshot = snapshot.CopySnapshotForPlayer(NewSnapshotId, HasHit);

            Snapshots.Add(newSnapshot);

            while (Snapshots.Count > MAX_SNAPSHOTS)
            {
                Snapshots.RemoveAt(0);
            }
        }

        public Snapshot GetSnapshot(int id)
        {
            return Snapshots.FirstOrDefault(x => x.Id == id);
        }

        public void AddSnapshotFromServer(Snapshot snapshot)
        {
            Snapshots.Add(snapshot);
            Snapshots.Sort(((snapshot1, snapshot2) => snapshot1.Id.CompareTo(snapshot2.Id)));
            while (Snapshots.Count > MAX_SNAPSHOTS)
            {
                Snapshots.RemoveAt(0);
            }

            ClientOffsetToServer = this.GetInterpolationGoal().ServerTime - DateTime.Now;
        }

        public DateTime GetEstimatedServerTime()
        {
            return DateTime.Now.Add(ClientOffsetToServer);
        }

        public Snapshot GetInterpolationGoal()
        {
            if (Snapshots.Count < 1)
                return new Snapshot();

            return Snapshots[Snapshots.Count - 1];
        }

        public Snapshot GetInterpolationOrigin()
        {
            if (Snapshots.Count < 2)
                return GetInterpolationGoal();

            return Snapshots[Snapshots.Count - 2];
        }
    }
}
