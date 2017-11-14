using System.Collections.Concurrent;
using System.Collections.Generic;
using GameSimulation.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using SimpleFPSShared;

namespace UdpDispatcher
{
    public class UdpEventListener : IUdpEventListener
    {
        private ConcurrentDictionary<long, int> peerToRoom = new ConcurrentDictionary<long, int>();
        private ConcurrentDictionary<long, string> peerToName = new ConcurrentDictionary<long, string>();
        public UdpManager UdpManager { get; set; }

        public void PollEvents()
        {
            UdpManager.PollEvents();
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            int roomId = 0;
            string name = string.Empty;
            if (peerToRoom.TryRemove(peer.ConnectId, out roomId) && peerToName.TryRemove(peer.ConnectId, out name))
            {
                IGameSimulation room = ActorProxy.Create<IGameSimulation>(new ActorId(peerToRoom.GetOrAdd(peer.ConnectId, roomId)));

                room.ProcessPlayerDisconnected(name);
            }
        }


        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            if (peer.ConnectionState != ConnectionState.Connected)
                return;

            SFPSPlayerCommand cmd = new SFPSPlayerCommand();
            cmd.Deserialize(reader.Data, 0);

            IGameSimulation room = ActorProxy.Create<IGameSimulation>(new ActorId(peerToRoom.GetOrAdd(peer.ConnectId, cmd.RoomId)));
            peerToName.GetOrAdd(peer.ConnectId, cmd.MyName);
            room.ProcessPacket(reader.Data).ContinueWith((data) =>
            {
                if(data.Result != null)
                    peer.Send(data.Result, ChannelType.ReliableOrdered);
            });
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