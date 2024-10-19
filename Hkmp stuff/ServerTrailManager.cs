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
            _netManager.TrailEvent += packet => OnTrail(packet.scene, packet.positionX, packet.positionY);
            _netManager.CleanEvent += packet => OnClean(packet.scene);
        }

        private void OnTrail(string scene, float positionX, float positionY) {
            _netManager.SendTrail(_serverApi.ServerManager.Players, scene, positionX, positionY);
        }

        private void OnClean(string scene) {
            _netManager.SendCleanup(_serverApi.ServerManager.Players, scene);
        }
    }
}
