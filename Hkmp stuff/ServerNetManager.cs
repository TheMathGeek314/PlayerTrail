using System;
using System.Collections.Generic;
using Hkmp.Api.Server;
using Hkmp.Api.Server.Networking;
using Hkmp.Networking.Packet;

namespace PlayerTrail {
    public class ServerNetManager {
        public event Action<TrailPacket> TrailEvent;
        public event Action<CleanPacket> CleanEvent;

        private readonly IServerAddonNetworkSender<ClientPacketId> _netSender;

        public ServerNetManager(ServerAddon addon, INetServer netServer) {
            _netSender = netServer.GetNetworkSender<ClientPacketId>(addon);
            var netReceiver = netServer.GetNetworkReceiver<ServerPacketId>(addon, InstantiatePacket);
            netReceiver.RegisterPacketHandler<TrailPacket>(
                ServerPacketId.TrailSeed,
                (id, packetData) => TrailEvent?.Invoke(packetData)
            );
            netReceiver.RegisterPacketHandler<CleanPacket>(
                ServerPacketId.CleanRoom,
                (id, packetData) => CleanEvent?.Invoke(packetData)
            );
        }

        public void SendTrail(IReadOnlyCollection<IServerPlayer> players, string scene, float positionX, float positionY) {
            foreach(var player in players) {
                try {
                    _netSender.SendSingleData(
                        ClientPacketId.TrailSeed,
                        new TrailPacket {
                            scene = scene,
                            positionX = positionX,
                            positionY = positionY
                        },
                        player.Id);
                }
                catch { }
            }
        }

        public void SendCleanup(IReadOnlyCollection<IServerPlayer> players, string scene) {
            foreach(var player in players) {
                try {
                    _netSender.SendSingleData(
                        ClientPacketId.CleanRoom,
                        new CleanPacket {
                            scene = scene
                        },
                        player.Id);
                }
                catch { }
            }
        }

        private static IPacketData InstantiatePacket(ServerPacketId packetId) {
            switch(packetId) {
                case ServerPacketId.TrailSeed:
                    return new TrailPacket();
                case ServerPacketId.CleanRoom:
                    return new CleanPacket();
            }
            return null;
        }
    }
}
