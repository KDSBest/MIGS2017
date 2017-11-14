using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReliableUdp;
using ReliableUdp.Logging;
using SimpleFPSShared;
using SimpleFPSShared.Client;
using Utility;

namespace BotConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            FactoryRegistrations.Register();
            bool isRunning = true;

            SFPSPlayerCommand cmd = new SFPSPlayerCommand();
            SFPSVector2 myCurrentPosition = new SFPSVector2();
            bool sendCmds = false;
            SFPSClient client = null;
            string name = "Bot" + Guid.NewGuid().ToString("N").Substring(0, 17);
            Random rnd = new Random();
            client = new SFPSClient("localhost", 5667, name, 0,
                () =>
                {
                    if (client == null)
                        return;

                    var goal = client.SnapshotManagement.GetInterpolationGoal();
                    var origin = client.SnapshotManagement.GetInterpolationOrigin();
                    Console.Clear();
                    Console.WriteLine("Player State Count: " + goal.Player.Count);

                    foreach (var pKV in goal.Player)
                    {
                        if (pKV.Key == name)
                            myCurrentPosition = pKV.Value.Position;
                        Console.WriteLine("Player '{0}': {1}", pKV.Key, pKV.Value);
                    }
                },
                () =>
                {
                    sendCmds = true;
                });
            client.Connect();

            while (isRunning)
            {
                client.Update();

                if (sendCmds)
                {
                    if (myCurrentPosition == cmd.State.Position)
                    { 
                        cmd.State.Position = new SFPSVector2()
                        {
                            X = rnd.Next(-100, 100),
                            Y = rnd.Next(-100, 100)
                        };
                    }

                    client.SendCmd(cmd);
                }
                Thread.Sleep(50);
            }
        }
    }
}
