using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace SimpleFPSShared.Client
{
    public class SFPSClient : IUdpEventListener
    {
        private string host;
        private int port;
        private string name;
        private int roomId;
        public SnapshotManagement SnapshotManagement = new SnapshotManagement();
        public ExtrapolationDataManagement ExtrapolationDataManagement = new ExtrapolationDataManagement();

        private Action newSnapshotsCallback;
        private Action connectCallback;

        public SFPSClient(string host, int port, string name, int roomId, Action newSnapshotsCallback, Action connectCallback)
        {
            this.host = host;
            this.port = port;
            this.name = name;
            this.roomId = roomId;
            this.newSnapshotsCallback = newSnapshotsCallback;
            this.connectCallback = connectCallback;
        }

        public void Update()
        {
            UdpManager.PollEvents();
        }

        public void Connect()
        {
            bool isConnected = false;
            bool connecting = true;
            UdpManager = new UdpManager(this, "kds");

            UdpManager.Connect(host, port);
        }

        public void SendCmd(SFPSPlayerCommand cmd)
        {
            cmd.MyName = name;
            cmd.RoomId = roomId;
            cmd.EstimatedServerTime = SnapshotManagement.GetEstimatedServerTime();
            ExtrapolationDataManagement.AddExtrapolationData(cmd);

            if (cmd.Shot.IsValid)
            {
                UdpManager.GetFirstPeer().Send(cmd.Serialize(), ChannelType.ReliableOrdered);
            }
            else
            {
                UdpManager.GetFirstPeer().Send(cmd.Serialize(), ChannelType.UnreliableOrdered);
            }
        }

        public UdpManager UdpManager { get; set; }

        public void OnPeerConnected(UdpPeer peer)
        {
            connectCallback();
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            this.Connect();
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            int offset = 0;
            while (offset < reader.Data.Length)
            {
                Snapshot snapshot = new Snapshot();
                offset = snapshot.Deserialize(reader.Data, offset);
                SnapshotManagement.AddSnapshotFromServer(snapshot);
            }

            newSnapshotsCallback();
        }

        public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
        {
        }
    }
}

