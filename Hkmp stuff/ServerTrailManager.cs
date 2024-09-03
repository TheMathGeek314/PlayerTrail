using System.Collections.Concurrent;
using Hkmp.Api.Server;

namespace PlayerTrail {
    public class ServerTrailManager {

        private readonly IServerApi _serverApi;
        private readonly ServerNetManager _netManager;

        public ServerTrailManager(TrailServerAddon addon, IServerApi serverApi) {
            _serverApi = serverApi;
            _netManager = new ServerNetManager(addon, serverApi.NetServer);
        }

        public void Initialize() {
            _netManager.TrailEvent += packet => OnTrail(packet.scene, packet.position);
            _netManager.CleanEvent += packet => OnClean(packet.scene);
        }

        private void OnTrail(string scene, UnityEngine.Vector3 position) {
            _netManager.SendTrail(_serverApi.ServerManager.Players, scene, position);
        }

        private void OnClean(string scene) {
            _netManager.SendCleanup(_serverApi.ServerManager.Players, scene);
        }
    }
}
