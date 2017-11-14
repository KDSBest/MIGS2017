using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FakeNetwork : MonoBehaviour
{
    private const string Statistic = @"Traffic to Server: {0} Kbps {1} Packet/s {2} Loss/s
Traffic to P1: {3} Kbps {4} Packet/s {5} Loss/s
Traffic to P2: {6} Kbps {7} Packet/s {8} Loss/s";
    public float LatencyMin = 0.03f;
    public float LatencyMax = 0.1f;

    public int PacketLoss = 10;

    public Text StatisticText;

    public enum Receiver
    {
        Server,
        P1,
        P2
    }

    private Dictionary<Receiver, Action<byte[]>> actions = new Dictionary<Receiver, Action<byte[]>>();

    private Dictionary<Receiver, List<Message>> queues = new Dictionary<Receiver, List<Message>>();
    private Dictionary<Receiver, FakeNetworkStatistic> statistics = new Dictionary<Receiver, FakeNetworkStatistic>();
    private Dictionary<Receiver, FakeNetworkStatistic> accumulatedStatistics = new Dictionary<Receiver, FakeNetworkStatistic>();
    private int lastCopySecond = -1;

    public void Start()
    {
        foreach (var receiver in GameObject.FindObjectsOfType<FakeNetworkReceiver>())
        {
            actions.Add(receiver.Receiver, receiver.Action);
            statistics.Add(receiver.Receiver, new FakeNetworkStatistic());
            accumulatedStatistics.Add(receiver.Receiver, new FakeNetworkStatistic());
        }
    }

    public void SendTo(Receiver receiver, byte[] data)
    {
        accumulatedStatistics[receiver].Kbps += data.Length / 1024.0f;
        accumulatedStatistics[receiver].SendPacketsPerSecond++;
        if (!queues.ContainsKey(receiver))
        {
            queues.Add(receiver, new List<Message>());
        }

        var drop = UnityEngine.Random.Range(0, 100);
        if (drop - PacketLoss < 0)
        {
            accumulatedStatistics[receiver].LostPacketsPerSecond++;
            return;
        }

        queues[receiver].Add(new Message()
        {
            Data = data,
            DueTime = UnityEngine.Random.Range(LatencyMin, LatencyMin)
        });
    }

    public void Update()
    {
        foreach (var queue in queues)
        {
            for (int i = 0; i < queue.Value.Count; i++)
            {
                queue.Value[i].DueTime -= Time.deltaTime;
                if (queue.Value[i].DueTime <= 0)
                {
                    if (actions.ContainsKey(queue.Key))
                        actions[queue.Key].Invoke(queue.Value[i].Data);

                    queue.Value.RemoveAt(i);
                    i--;
                }
            }
        }

        if (lastCopySecond != DateTime.Now.Second)
        {
            lastCopySecond = DateTime.Now.Second;
            foreach (var key in statistics.Keys.ToList())
            {
                statistics[key] = accumulatedStatistics[key];
                accumulatedStatistics[key] = new FakeNetworkStatistic();
            }

            if (StatisticText != null)
            {
                StatisticText.text = string.Format(Statistic, statistics[Receiver.Server].Kbps, statistics[Receiver.Server].SendPacketsPerSecond, statistics[Receiver.Server].LostPacketsPerSecond,
                statistics[Receiver.P1].Kbps, statistics[Receiver.P1].SendPacketsPerSecond, statistics[Receiver.P1].LostPacketsPerSecond,
                statistics[Receiver.P2].Kbps, statistics[Receiver.P2].SendPacketsPerSecond, statistics[Receiver.P2].LostPacketsPerSecond);
            }
        }
    }

}
