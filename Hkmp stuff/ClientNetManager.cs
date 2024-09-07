using System;
using Hkmp.Api.Client;
using Hkmp.Api.Client.Networking;
using Hkmp.Networking.Packet;

namespace PlayerTrail {
    public class ClientNetManager {
        public event Action<TrailPacket> TrailEvent;
        public event Action<CleanPacket> CleanEvent;

        private readonly IClientAddonNetworkSender<ServerPacketId> _netSender;

        public ClientNetManager(ClientAddon addon, INetClient netClient) {
            _netSender = netClient.GetNetworkSender<ServerPacketId>(addon);
            var netReceiver = netClient.GetNetworkReceiver<ClientPacketId>(addon, InstantiatePacket);
            netReceiver.RegisterPacketHandler<TrailPacket>(
                ClientPacketId.TrailSeed,
                packetData => TrailEvent?.Invoke(packetData)
            );
            netReceiver.RegisterPacketHandler<CleanPacket>(
                ClientPacketId.CleanRoom,
                packetData => CleanEvent?.Invoke(packetData)
            );
        }

        public void SendTrail(string scene, float positionX, float positionY) {
            _netSender.SendSingleData(
                ServerPacketId.TrailSeed,
                new TrailPacket {
                    scene = scene,
                    positionX = positionX,
                    positionY = positionY
                }
            );
        }

        public void SendCleanup(string scene) {
            _netSender.SendSingleData(
                ServerPacketId.CleanRoom,
                new CleanPacket {
                    scene = scene
                }
            );
        }

        private static IPacketData InstantiatePacket(ClientPacketId packetId) {
            switch(packetId) {
                case ClientPacketId.TrailSeed:
                    return new TrailPacket();
                case ClientPacketId.CleanRoom:
                    return new CleanPacket();
            }
            return null;
        }
    }
}
