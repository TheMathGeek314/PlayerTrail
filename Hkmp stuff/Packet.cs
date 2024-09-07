using Hkmp.Networking.Packet;

namespace PlayerTrail {
    public class TrailPacket: IPacketData {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => false;

        public string scene { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }

        public void WriteData(IPacket packet) {
            packet.Write(scene);
            packet.Write(positionX);
            packet.Write(positionY);
        }

        public void ReadData(IPacket packet) {
            scene = packet.ReadString();
            positionX = packet.ReadFloat();
            positionY = packet.ReadFloat();
        }
    }

    public class CleanPacket: IPacketData {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;

        public string scene{ get; set; }

        public void WriteData(IPacket packet) {
            packet.Write(scene);
        }
        public void ReadData(IPacket packet) {
            scene = packet.ReadString();
        }
    }
}
