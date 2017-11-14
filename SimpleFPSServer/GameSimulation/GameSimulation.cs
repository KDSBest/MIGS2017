using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GameSimulation.Interfaces;
using SimpleFPSShared;

namespace GameSimulation
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Volatile)]
    internal class GameSimulation : Actor, IGameSimulation
    {
        public const float MaxDistanceBetweenSnapshots = 0.1f;

        public Snapshot MasterSnapshot = new Snapshot();

        public Dictionary<string, SnapshotManagement> SnapshotManagements = new Dictionary<string, SnapshotManagement>();

        /// <summary>
        /// Initializes a new instance of GameSimulation
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public GameSimulation(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return base.OnActivateAsync();
        }

        public async Task<byte[]> ProcessPacket(byte[] data)
        {
            List<SFPSPlayerCommand> commands = new List<SFPSPlayerCommand>();
            int offset = 0;
            while (offset < data.Length)
            {
                SFPSPlayerCommand newCmd = new SFPSPlayerCommand();
                offset = newCmd.Deserialize(data, offset);
                commands.Add(newCmd);
            }

            if (commands.Count == 0)
            {
                return null;
            }

            commands.Sort(((command1, command2) => command1.EstimatedServerTime.CompareTo(command2.EstimatedServerTime)));
            string name = commands[0].MyName;
            if (!SnapshotManagements.ContainsKey(name))
                SnapshotManagements.Add(name, new SnapshotManagement());

            ProcessMovement(commands);
            ProcessShots(commands);

            SnapshotManagements[name].GenerateFromMasterSnapshot(MasterSnapshot);

            var playerSnapshot = SnapshotManagements[name].GetInterpolationGoal();

            return playerSnapshot.Serialize();
        }

        public async Task ProcessPlayerDisconnected(string name)
        {
            if (MasterSnapshot.Player.ContainsKey(name))
                MasterSnapshot.Player.Remove(name);
            if (MasterSnapshot.Shots.ContainsKey(name))
                MasterSnapshot.Shots.Remove(name);
        }

        private void ProcessShots(List<SFPSPlayerCommand> commands)
        {
            if (commands.Count < 1)
                return;

            foreach (var cmd in commands)
            {
                if(cmd.Shot == null || !cmd.Shot.IsValid)
                    continue;

                var snapshotManagement = SnapshotManagements[commands[0].MyName];
                snapshotManagement.HasHit = false;

                var goal = snapshotManagement.GetSnapshot(cmd.Shot.InterpolationDestinationId);
                var origin = snapshotManagement.GetSnapshot(cmd.Shot.InterpolationOriginId);

                if(origin == null || goal == null)
                    continue;

                if (cmd.Shot.InterpolationPercentage > 100)
                    cmd.Shot.InterpolationPercentage = 100;
                if (cmd.Shot.InterpolationPercentage < 0)
                    cmd.Shot.InterpolationPercentage = 0;

                float t = ((float) cmd.Shot.InterpolationPercentage) / 100.0f;
                SFPSRay ray = cmd.Shot.Ray;
                foreach (var player in goal.Player)
                {
                    if(player.Key == cmd.MyName)
                        continue;

                    var goalPosition = new SFPSVector3(player.Value.Position.X, 0, player.Value.Position.Y);

                    if (origin.Player.ContainsKey(player.Key))
                    {
                        var originPosition = new SFPSVector3(origin.Player[player.Key].Position.X, 0, origin.Player[player.Key].Position.Y);
                        goalPosition = originPosition + ((goalPosition - originPosition) * t);
                    }

                    float intersectionT;
                    SFPSVector3 p;
                    if (ray.IntersectSphere(goalPosition, 0.5f, out intersectionT, out p))
                    {
                        snapshotManagement.HasHit = true;
                        // For showcase I just test the first hit (no matter if it's the nearest or so)
                        break;
                    }
                }
            }
        }

        private void ProcessMovement(List<SFPSPlayerCommand> commands)
        {
            var lastCmd = commands.Last();
            if (lastCmd.EstimatedServerTime > DateTime.Now)
                lastCmd.EstimatedServerTime = DateTime.Now;

            if (!MasterSnapshot.Player.ContainsKey(lastCmd.MyName))
            {
                MasterSnapshot.Player.Add(lastCmd.MyName, lastCmd.State);
                return;
            }

            var snapshot = SnapshotManagements[lastCmd.MyName].GetInterpolationGoal();
            if (!snapshot.Player.ContainsKey(lastCmd.MyName))
            {
                MasterSnapshot.Player[lastCmd.MyName] = lastCmd.State;
                return;
            }

            // ANTI SPEED HACK!
            var oldState = snapshot.Player[lastCmd.MyName];
            var oldTime = snapshot.ServerTime;

            var passedTime = lastCmd.EstimatedServerTime - oldTime;
            var passedDistance = Math.Abs(SFPSVector2.Distance(lastCmd.State.Position, oldState.Position));
            var passedSnapshots = passedTime.TotalMilliseconds / 0.5f;

            // We ignore too old stuff
            if (passedSnapshots < 0)
                return;

            var maxDistance = MaxDistanceBetweenSnapshots * passedSnapshots;
            if (passedDistance <= maxDistance)
            {
                MasterSnapshot.Player[lastCmd.MyName] = lastCmd.State;
                return;
            }

            lastCmd.State.Position = oldState.Position + (SFPSVector2.Normalize(lastCmd.State.Position - oldState.Position) * (float)maxDistance);
            MasterSnapshot.Player[lastCmd.MyName] = lastCmd.State;
        }
    }
}
