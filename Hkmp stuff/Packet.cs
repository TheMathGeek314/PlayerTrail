using Hkmp.Networking.Packet;

namespace PlayerTrail {
    public class TrailPacket: IPacketData {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => false;

        public string scene { get; set; }
        public UnityEngine.Vector3 position { get; set; }

        public void WriteData(IPacket packet) {
            packet.Write(scene);
            packet.Write(position.x);
            packet.Write(position.y);
        }

        public void ReadData(IPacket packet) {
            scene = packet.ReadString();
            position = new UnityEngine.Vector3(packet.ReadFloat(), packet.ReadFloat(), 0.0067f);
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
